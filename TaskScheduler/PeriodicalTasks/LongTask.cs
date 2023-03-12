namespace TaskScheduler.PeriodicalTasks;

public class LongTask : IPeriodicalTask
{
    public TimeSpan LaunchPeriod => TimeSpan.FromSeconds(30);
    public async Task Start()
    {
        Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Long task started");
        await Task.Delay(20000);
        Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Long task completed");
    }
}