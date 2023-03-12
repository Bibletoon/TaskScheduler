using TaskScheduler.Models;

namespace TaskScheduler.Services.TaskStateStorage;

public class InMemoryTaskStateStorage : ITaskStateStorage
{
    private readonly Dictionary<string, DateTime> _data = new Dictionary<string, DateTime>();

    public List<TaskState> GetAll()
    {
        return _data.Select(kvp => new TaskState(kvp.Key, kvp.Value)).ToList();
    }

    public void Remove(string name)
    {
        _data.Remove(name);
    }

    public void Save(List<TaskState> states)
    {
        states.ForEach(s => _data[s.Name] = s.NextLaunch);
    }
}