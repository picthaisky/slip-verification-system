using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SlipVerification.Application.Interfaces.MessageQueue;

namespace SlipVerification.Infrastructure.MessageQueue;

/// <summary>
/// Publisher for sending messages to RabbitMQ queues
/// </summary>
public class RabbitMQPublisher : IMessagePublisher
{
    private readonly IRabbitMQConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMQPublisher> _logger;

    public RabbitMQPublisher(
        IRabbitMQConnectionFactory connectionFactory,
        ILogger<RabbitMQPublisher> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: properties,
            body: body
        );

        _logger.LogInformation(
            "Published message {MessageId} to queue {QueueName}",
            properties.MessageId,
            queueName
        );

        await Task.CompletedTask;
    }

    public async Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default) where T : class
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            basicProperties: properties,
            body: body
        );

        _logger.LogInformation(
            "Published message {MessageId} to exchange {Exchange} with routing key {RoutingKey}",
            properties.MessageId,
            exchange,
            routingKey
        );

        await Task.CompletedTask;
    }
}
