namespace SlipVerification.Infrastructure.MessageQueue;

/// <summary>
/// Configuration for RabbitMQ connection
/// </summary>
public class RabbitMQConfiguration
{
    public const string SectionName = "RabbitMQ";
    
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public int RetryCount { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 5;
}
