using TaskScheduler.Models;

namespace TaskScheduler.Services.TaskStateStorage;

public interface ITaskStateStorage
{
    List<TaskState> GetAll();
    void Remove(string name);
    void Save(List<TaskState> states);
}