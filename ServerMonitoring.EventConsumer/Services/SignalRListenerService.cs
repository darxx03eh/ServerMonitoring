using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServerMonitoring.EventConsumer.Configs;
using ServerMonitoring.EventConsumer.Models;

namespace ServerMonitoring.EventConsumer.Services;

public class SignalRListenerService : BackgroundService
{
    private readonly SignalRConfig _options;
    private readonly ILogger<SignalRListenerService> _logger;
    private HubConnection? _connection;

    public SignalRListenerService(IOptions<SignalRConfig> options, ILogger<SignalRListenerService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(_options.SignalRUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<AlertMessage>("AnomalyAlert", PrintAlert);
        _connection.On<AlertMessage>("HighUsageAlert", PrintAlert);

        _connection.Reconnecting += error =>
        {
            _logger.LogWarning("Connection lost, attempting to reconnect... {Error}", error?.Message);
            return Task.CompletedTask;
        };

        _connection.Reconnected += connectionId =>
        {
            _logger.LogInformation("Reconnected. ConnectionId: {Id}", connectionId);
            return Task.CompletedTask;
        };

        _connection.Closed += error =>
        {
            _logger.LogError(error, "Connection closed permanently.");
            return Task.CompletedTask;
        };

        await ConnectWithRetryAsync(stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ConnectWithRetryAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _connection!.StartAsync(stoppingToken);
                _logger.LogInformation("Connected to SignalR hub at {Url}", _options.SignalRUrl);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to connect ({Message}). Retrying in 5s...", ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private void PrintAlert(AlertMessage alert)
    {
        Console.WriteLine(
            $"[{alert.Timestamp:yyyy-MM-dd HH:mm:ss} UTC] {alert.AlertType} | " +
            $"Server={alert.ServerIdentifier} | Metric={alert.Metric} | {alert.Message}");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null)
            await _connection.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}