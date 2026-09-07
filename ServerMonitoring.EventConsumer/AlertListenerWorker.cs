using ServerMonitoring.EventConsumer.Models;
using ServerMonitoring.SignalR.Interfaces.Services;

namespace ServerMonitoring.EventConsumer;

public class AlertListenerWorker(INotificationListener listener) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await listener.SubscribeAsync<AlertMessage>("AnomalyAlert", (alert, ct) => PrintAlert(alert), stoppingToken);
        await listener.SubscribeAsync<AlertMessage>("HighUsageAlert", (alert, ct) => PrintAlert(alert), stoppingToken);

        await listener.StartAsync(stoppingToken);
        
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    
    private Task PrintAlert(AlertMessage alert)
    {
        Console.WriteLine(
            $"[{alert.Timestamp:yyyy-MM-dd HH:mm:ss} UTC] {alert.AlertType} | " +
            $"Server={alert.ServerIdentifier} | Metric={alert.Metric} | {alert.Message}");

        return Task.CompletedTask;
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await listener.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}