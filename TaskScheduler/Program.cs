using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using StackExchange.Redis;
using TaskScheduler.Services;
using TaskScheduler.Services.TaskStateStorage;
using TaskScheduler.Utils;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();

        Log.Logger = new LoggerConfiguration() // initiate the logger configuration
            .ReadFrom.Configuration(configuration) // connect serilog to our configuration folder
            .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog 
            .WriteTo.Console() // decide where the logs are going to be shown
            .CreateLogger(); //initialise the logger

        Log.Logger.Information("Application Starting");
        
        services.AddSingleton<Scheduler>();
        services.Configure<SchedulerConfiguration>(configuration.GetRequiredSection("SchedulerConfiguration"));
        services.AddSingleton<IServiceProvider>(sp => sp);

        var multiplexer = ConnectionMultiplexer.Connect(configuration.GetRequiredSection("RedisConnectionString").Value);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        services.AddSingleton<ITaskStateStorage, RedisTaskStateStorage>();
    })
    .UseSerilog()
    .Build();
    
var scheduler = host.Services.GetService<Scheduler>();
scheduler.Start();

await host.WaitForShutdownAsync();