namespace TaskScheduler.PeriodicalTasks;

public interface IPeriodicalTask
{
    TimeSpan LaunchPeriod { get; }
    Task Start();
}