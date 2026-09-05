using ServerMonitoring.MessageQueue.DependencyInjection;
using ServerMonitoring.Processor.Alerts;
using ServerMonitoring.Processor.Alerts.Hubs;
using ServerMonitoring.Processor.Interfaces.Repositories;
using ServerMonitoring.Processor.Options;
using ServerMonitoring.Processor.Repositories;
using ServerMonitoring.Processor.Services;

namespace ServerMonitoring.Processor;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();
        
        builder.Services.Configure<AnomalyDetectionOptions>(
            builder.Configuration.GetSection("AnomalyDetectionConfig"));
        builder.Services.Configure<MongoOptions>(
            builder.Configuration.GetSection("MongoConfig"));

        builder.Services.AddRabbitMqMessaging(builder.Configuration);

        builder.Services.AddSignalR();
        builder.Services.AddSingleton<IServerStatisticsRepository, MongoServerStatisticsRepository>();
        builder.Services.AddSingleton<IAlertNotifier, SignalRAlertNotifier>();
        builder.Services.AddHostedService<AnomalyDetectionService>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();
        app.MapHub<AlertsHub>("/hubs/alerts");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}