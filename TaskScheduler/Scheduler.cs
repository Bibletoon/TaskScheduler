using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaskScheduler.PeriodicalTasks;

namespace TaskScheduler;

public class Scheduler
{
    private readonly Timer _timer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITaskStateStorage _taskStateStorage;
    private readonly SchedulerConfiguration _configuration;

    public Scheduler(ITaskStateStorage taskStateStorage, IServiceProvider serviceProvider, IOptions<SchedulerConfiguration> configuration)
    {
        _taskStateStorage = taskStateStorage;
        _serviceProvider = serviceProvider;
        _timer = new Timer((state) => { RunTasks(); }, null, Timeout.Infinite, Timeout.Infinite);
        _configuration = configuration.Value;
    }

    public void Start()
    {
        var tasks = TaskScanner.GetAllTasks()
            .Where(t => _configuration.Tasks.Contains(t.Name))
            .ToList();
        
        ScheduleTasks(tasks);
    }
    
    private void RunTasks()
    {
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
        _timer.Change((int)delay.TotalMilliseconds, Timeout.Infinite);
        _taskStateStorage.Save(tasks);
    }
}