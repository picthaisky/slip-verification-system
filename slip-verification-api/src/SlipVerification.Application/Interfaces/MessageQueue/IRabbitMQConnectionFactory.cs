using RabbitMQ.Client;

namespace SlipVerification.Application.Interfaces.MessageQueue;

/// <summary>
/// Factory for creating RabbitMQ connections
/// </summary>
public interface IRabbitMQConnectionFactory
{
    /// <summary>
    /// Creates or returns an existing RabbitMQ connection
    /// </summary>
    IConnection CreateConnection();
}
