namespace SmartSure.Shared.Messaging;

/// <summary>
/// Configuration options for the RabbitMQ connection used by MassTransit.
/// </summary>
public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "smartsure.events";
}
