using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TaskScheduler;
using TaskScheduler.Services;
using TaskScheduler.Services.TaskStateStorage;
using TaskScheduler.Utils;

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();

var services = new ServiceCollection();
services.AddSingleton<Scheduler>();
services.Configure<SchedulerConfiguration>(configuration.GetRequiredSection("SchedulerConfiguration"));
services.AddSingleton<IServiceProvider>(sp => sp);

var multiplexer = ConnectionMultiplexer.Connect(configuration.GetRequiredSection("RedisConnectionString").Value);
services.AddSingleton<IConnectionMultiplexer>(multiplexer);
services.AddSingleton<ITaskStateStorage, RedisTaskStateStorage>();


var provider = services.BuildServiceProvider();
var scheduler = provider.GetService<Scheduler>();
scheduler.Start();

await Task.Delay(-1);