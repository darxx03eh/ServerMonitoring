using ServerMonitoring.MessageQueue.DependencyInjection;
using ServerMonitoring.StatsCollector.IProviders;
using ServerMonitoring.StatsCollector.IProviders.Factory;
using ServerMonitoring.StatsCollector.Options;
using ServerMonitoring.StatsCollector.Providers;

namespace ServerMonitoring.StatsCollector;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<StatisticsCollectionWorker>();
        
        builder.Services.Configure<ServerStatisticsOptions>(
            builder.Configuration.GetSection("ServerStatisticsConfig"));

        builder.Services.AddRabbitMqMessaging(builder.Configuration);

        builder.Services.AddSingleton<IServerStatisticsProvider>(_ => ServerStatisticsProviderFactory.Create());
        builder.Services.AddHostedService<StatisticsCollectionWorker>();

        var host = builder.Build();
        host.Run();
    }
}