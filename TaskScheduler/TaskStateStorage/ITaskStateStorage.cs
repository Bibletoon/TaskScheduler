namespace TaskScheduler;

public interface ITaskStateStorage
{
    List<TaskState> GetAll();
    void Save(List<TaskState> states);
}