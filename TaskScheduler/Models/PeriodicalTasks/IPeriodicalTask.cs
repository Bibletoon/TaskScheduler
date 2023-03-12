namespace TaskScheduler.Models.PeriodicalTasks;

public interface IPeriodicalTask
{
    TimeSpan LaunchPeriod { get; }
    Task Start();
}