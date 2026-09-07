using ServerMonitoring.MessageQueue.DependencyInjection;
using ServerMonitoring.Processor.Alerts;
using ServerMonitoring.Processor.Interfaces.Repositories;
using ServerMonitoring.Processor.Options;
using ServerMonitoring.Processor.Repositories;

namespace ServerMonitoring.Processor;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Services.Configure<AnomalyDetectionOptions>(
            builder.Configuration.GetSection("AnomalyDetectionConfig"));
        builder.Services.Configure<MongoOptions>(
            builder.Configuration.GetSection("MongoConfig"));
        
        builder.Services.AddRabbitMqMessaging(builder.Configuration);
        
        builder.Services.AddSingleton<IServerStatisticsRepository, MongoServerStatisticsRepository>();

        builder.Services.AddHttpClient<IAlertNotifier, HttpAlertNotifier>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["NotificationService:BaseUrl"]!);
        });
        builder.Services.AddHostedService<AnomalyDetectionService>();

        var host = builder.Build();
        host.Run();
    }
}