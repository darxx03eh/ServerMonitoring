namespace ServerMonitoring.MessageQueue.Options;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "server.events";
    public string? DeadLetterExchangeName { get; set; }
    public bool DurableExchange { get; set; } = true;
    public string? QueueName { get; set; }
    public string? DeadLetterQueueName { get; set; }
    public bool DurableQueue { get; set; } = true;
    public ushort PrefetchCount { get; set; } = 10;
    public string? DeadLetterRoutingKey { get; set; }
}