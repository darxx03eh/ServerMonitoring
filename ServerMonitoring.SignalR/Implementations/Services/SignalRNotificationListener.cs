using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServerMonitoring.SignalR.Interfaces.Services;
using ServerMonitoring.SignalR.Options;

namespace ServerMonitoring.SignalR.Implementations.Services;

public class SignalRNotificationListener : INotificationListener
{
    private readonly SignalRListenerOptions _options;
    private readonly ILogger<SignalRNotificationListener> _logger;
    private readonly HubConnection _connection;
    public SignalRNotificationListener(IOptions<SignalRListenerOptions> options,
        ILogger<SignalRNotificationListener> logger)
    {
        _options = options.Value;
        _logger = logger;

        _connection = new HubConnectionBuilder()
            .WithUrl(_options.HubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.Reconnecting += error =>
        {
            _logger.LogError("Connection lost, attempting to reconnect... {Error}", error?.Message);
            return Task.CompletedTask;
        };

        _connection.Reconnected += connectionId =>
        {
            _logger.LogInformation("Reconnected. ConnectionId: {Id}", connectionId);
            return Task.CompletedTask;
        };
        
        _connection.Closed += error =>
        {
            logger.LogError(error, "Connection closed permanently.");
            return Task.CompletedTask;
        };
    }

    public Task SubscribeAsync<T>(string @event, Func<T, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _connection.StartAsync(cancellationToken);
                _logger.LogInformation("Connected to SignalR hub at {Url}", _options.HubUrl);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "Failed to connect ({Message}). Retrying in {Seconds}s...",
                    ex.Message, _options.RetryDelaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(_options.RetryDelaySeconds), cancellationToken);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}