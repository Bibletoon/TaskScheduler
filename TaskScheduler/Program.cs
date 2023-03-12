using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskScheduler;

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<Scheduler>();
serviceCollection.Configure<SchedulerConfiguration>(configuration.GetSection("SchedulerConfiguration"));
serviceCollection.AddSingleton<IServiceProvider>(sp => sp);
serviceCollection.AddSingleton<ITaskStateStorage, InMemoryTaskStateStorage>();
var provider = serviceCollection.BuildServiceProvider();
var scheduler = provider.GetService<Scheduler>();
scheduler.Start();

await Task.Delay(-1);