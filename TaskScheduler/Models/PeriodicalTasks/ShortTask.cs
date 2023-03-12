namespace TaskScheduler.Models.PeriodicalTasks;

public class ShortTask : IPeriodicalTask
{
    public TimeSpan LaunchPeriod => TimeSpan.FromSeconds(5);
    public async Task Start()
    {
        Console.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Short task!");
    }
}