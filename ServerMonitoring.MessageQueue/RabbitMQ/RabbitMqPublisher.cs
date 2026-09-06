using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ServerMonitoring.MessageQueue.IRabbitMQ;
using ServerMonitoring.MessageQueue.Options;

namespace ServerMonitoring.MessageQueue.RabbitMQ;

public class RabbitMqPublisher(IOptions<RabbitMqOptions> options) : IMessagePublisher
{
    private readonly RabbitMqOptions _options
        = options.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;
    
    public async Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic must not be null or empty.", nameof(topic));
        
        var channel = await GetOrCreateChannelAsync(cancellationToken).ConfigureAwait(false);
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(message);
        
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };
        
        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: topic,
            mandatory: false,
            basicProperties: properties,
            body: payload,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task<IChannel> GetOrCreateChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
            return _channel;

        await _initLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _options.HostName,
                UserName = _options.UserName,
                Password = _options.Password,
                Port = _options.Port,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };
            
            _connection = await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            await _channel.ExchangeDeclareAsync(_options.ExchangeName, ExchangeType.Topic, _options.DurableExchange,
                autoDelete: false, cancellationToken: cancellationToken);

            return _channel;
        }
        finally
        {
            _initLock.Release();
        }
    }
    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync().ConfigureAwait(false);
        if (_connection is not null)
            await _connection.DisposeAsync().ConfigureAwait(false);
        _initLock.Dispose();
    }
}