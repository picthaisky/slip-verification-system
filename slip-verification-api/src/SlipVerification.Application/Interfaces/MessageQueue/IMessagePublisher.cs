namespace SlipVerification.Application.Interfaces.MessageQueue;

/// <summary>
/// Interface for publishing messages to RabbitMQ queues
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to a queue
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="queueName">Target queue name</param>
    /// <param name="message">Message to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publishes a message to an exchange with routing key
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="message">Message to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default) where T : class;
}
