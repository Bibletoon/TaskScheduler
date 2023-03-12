using StackExchange.Redis;

namespace TaskScheduler;

public class RedisTaskStateStorage : ITaskStateStorage
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisTaskStateStorage(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public List<TaskState> GetAll()
    {
        var db = _connectionMultiplexer.GetDatabase();
        var server = _connectionMultiplexer.GetServers()[0];
        var keys = server.Keys(db.Database);
        return keys.Select(k => new TaskState(k.ToString(), DateTime.FromFileTimeUtc(long.Parse(db.StringGet(k).ToString())))).ToList();
    }

    public void Remove(string name)
    {
        var db = _connectionMultiplexer.GetDatabase();
        db.KeyDelete(name);
    }

    public void Save(List<TaskState> states)
    {
        var db = _connectionMultiplexer.GetDatabase();
        states.ForEach(state => db.StringSet(state.Name, state.NextLaunch.ToFileTimeUtc()));
    }
}