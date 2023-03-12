using TaskScheduler.Models.PeriodicalTasks;

namespace TaskScheduler.Utils;

public static class TaskScanner
{
    private static readonly Dictionary<string, Type> Cache = new Dictionary<string, Type>();

    public static List<Type> GetAllTasks()
    {
        var interfaceType = typeof(IPeriodicalTask);
        var tasks = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsAssignableTo(interfaceType))
            .ToList();
        
        tasks.ForEach(t => Cache[t.Name] = t);
        return tasks;
    }

    public static Type? GetTask(string name)
    {
        return Cache.ContainsKey(name) 
            ? Cache[name] 
            : GetAllTasks().FirstOrDefault(t => t.Name == name);
    }
}