using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskScheduler.Models;
using TaskScheduler.Models.PeriodicalTasks;
using TaskScheduler.Services.TaskStateStorage;
using TaskScheduler.Utils;

namespace TaskScheduler.Services;

public class Scheduler
{
    private readonly SemaphoreSlim _semaphore;
    private readonly Timer _timer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITaskStateStorage _taskStateStorage;
    private readonly IOptionsMonitor<SchedulerConfiguration> _configuration;
    private readonly ILogger<Scheduler> _logger;

    public Scheduler(ITaskStateStorage taskStateStorage, IServiceProvider serviceProvider, IOptionsMonitor<SchedulerConfiguration> configuration, ILogger<Scheduler> logger)
    {
        _taskStateStorage = taskStateStorage;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;

        _semaphore = new SemaphoreSlim(1, 1);
        _timer = new Timer((state) => { RunTasks(); }, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        _configuration.OnChange(_ => UpdateTasksList());
        var initialTasks = _configuration.Get("").Tasks;
        var tasks = TaskScanner.GetAllTasks()
            .Where(t => initialTasks.Contains(t.Name))
            .ToList();
        
        _logger.LogInformation("Registered tasks {tasks}", (object)tasks.Select(t => t.Name).ToArray());
        
        if (tasks.Count != initialTasks.Count)
            _logger.LogWarning("Unable to find tasks {tasks}", 
                (object)initialTasks.Where(it => tasks.All(t => t.Name != it)));
        
        ScheduleTasks(tasks);
    }

    private void UpdateTasksList()
    {
        var tasks = _configuration.Get("").Tasks;
        var registeredTasks = _taskStateStorage.GetAll();
        _semaphore.Wait();
        foreach (var taskToRemove in registeredTasks.Where(t => !tasks.Contains(t.Name)))
        {
            _taskStateStorage.Remove(taskToRemove.Name);
            _logger.LogInformation("Remove task {task}", taskToRemove.Name);
        }

        var newTasks = new List<Type>();

        foreach (var newTask in tasks.Where(t => registeredTasks.All(ts => ts.Name != t)))
        {
            var newTaskType = TaskScanner.GetTask(newTask);
            
            if (newTaskType is null)
            {
                _logger.LogWarning("Unable to find task {task}", newTask);
                continue;
            }

            newTasks.Add(newTaskType);
            _logger.LogInformation("Add new task {task}", newTask);
        }
        
        ScheduleTasks(newTasks);
        _semaphore.Release();
    }

    private void RunTasks()
    {
        _semaphore.Wait();
        var tasksSate = _taskStateStorage.GetAll();

        var currentTime = DateTime.UtcNow;
        var launchTasks = tasksSate
            .Where(t => t.NextLaunch <= currentTime)
            .Select(t => TaskScanner.GetTask(t.Name))
            .ToList();

        foreach (var task in launchTasks)
        {
            ExecuteTask(task);
        }

        ScheduleTasks(launchTasks);
        _semaphore.Release();
    }

    private void ExecuteTask(Type taskType)
    {
        var taskInstance = (IPeriodicalTask)ActivatorUtilities.CreateInstance(_serviceProvider, taskType);
        taskInstance.Start();
    }

    private void ScheduleTasks(List<Type> taskTypes)
    {
        var currentTime = DateTime.UtcNow;
        var newTasks = taskTypes.Select(t =>
        {
            var taskInstance = (IPeriodicalTask)ActivatorUtilities.CreateInstance(_serviceProvider, t);
            return new TaskState(t.Name, currentTime.Add(taskInstance.LaunchPeriod));
        }).ToList();
        
        _taskStateStorage.Save(newTasks);
        var tasks = _taskStateStorage.GetAll();
        var nearestTask = tasks.MinBy(t => t.NextLaunch);
        if (nearestTask is null)
            return;
        
        var delay = nearestTask.NextLaunch - currentTime;
        delay = delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
        _timer.Change((int)delay.TotalMilliseconds, Timeout.Infinite);
        _taskStateStorage.Save(tasks);
    }
}