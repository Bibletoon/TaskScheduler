namespace TaskScheduler;

public interface ITaskStateStorage
{
    List<TaskState> GetAll();
    void Remove(string name);
    void Save(List<TaskState> states);
}