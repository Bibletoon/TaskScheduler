namespace TaskScheduler.PeriodicalTasks;

public class ConsoleWriteTask : IPeriodicalTask
{
    public TimeSpan LaunchPeriod => TimeSpan.FromSeconds(10);
    public Task Start()
    {
        Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Task!");
        return Task.CompletedTask;
    }
}