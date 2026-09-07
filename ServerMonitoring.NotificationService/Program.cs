using ServerMonitoring.NotificationService.Models;
using ServerMonitoring.SignalR.DependencyInjection;
using ServerMonitoring.SignalR.Hubs;
using ServerMonitoring.SignalR.Interfaces.Services;

namespace ServerMonitoring.NotificationService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSignalRHubHosting();

        var app = builder.Build();
        app.MapHub<NotificationsHub>("/hubs/alerts");
        
        app.MapPost("/alerts", async (AlertMessage alert, INotificationPublisher publisher, CancellationToken ct) =>
        {
            await publisher.PublishAsync(alert.AlertType, alert, ct);
            return Results.Accepted();
        });
        
        app.MapGet("/health", () => Results.Ok("healthy"));

        await app.RunAsync();
    }
}