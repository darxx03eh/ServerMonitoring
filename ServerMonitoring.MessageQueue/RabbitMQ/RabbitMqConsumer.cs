using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServerMonitoring.MessageQueue.IRabbitMQ;
using ServerMonitoring.MessageQueue.Options;

namespace ServerMonitoring.MessageQueue.RabbitMQ;

public class RabbitMqConsumer(IOptions<RabbitMqOptions> options) : IMessageConsumer
{
    private readonly RabbitMqOptions _options
        = options.Value ?? throw new ArgumentNullException(nameof(options));
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task SubscribeAsync<T>(string topicPattern, Func<T, string, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(topicPattern))
            throw new ArgumentException("Topic pattern must not be null or empty.", nameof(topicPattern));
        
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };
        
        _connection = await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        
        await _channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: _options.DurableExchange,
            autoDelete: false,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        
        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _options.PrefetchCount,
            global: false,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        
        bool exclusive = string.IsNullOrWhiteSpace(_options.QueueName);
        var declareResult = await _channel.QueueDeclareAsync(
            queue: _options.QueueName ?? string.Empty,
            durable: !exclusive && _options.DurableQueue,
            exclusive: exclusive,
            autoDelete: exclusive,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        
        string queueName = declareResult.QueueName;

        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: _options.ExchangeName,
            routingKey: topicPattern,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                T? payload = JsonSerializer.Deserialize<T>(args.Body.Span);
                if (payload is not null)
                    await handler(payload, args.RoutingKey, cancellationToken).ConfigureAwait(false);
                
                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch
            {
                await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken)
                    .ConfigureAwait(false);
            }
        };
        
        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync().ConfigureAwait(false);
        if (_connection is not null)
            await _connection.DisposeAsync().ConfigureAwait(false);
    }
}