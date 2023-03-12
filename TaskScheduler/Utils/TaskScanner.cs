using TaskScheduler.Models.PeriodicalTasks;

namespace TaskScheduler.Utils;

public static class TaskScanner
{
    public static List<Type> GetAllTasks()
    {
        var interfaceType = typeof(IPeriodicalTask);
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsAssignableTo(interfaceType))
            .ToList();
    }

    public static Type GetTask(string name)
    {
        return GetAllTasks().FirstOrDefault(t => t.Name == name);
    }
}