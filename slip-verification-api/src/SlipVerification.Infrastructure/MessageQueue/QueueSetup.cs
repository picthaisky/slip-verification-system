using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SlipVerification.Application.Interfaces.MessageQueue;

namespace SlipVerification.Infrastructure.MessageQueue;

/// <summary>
/// Sets up RabbitMQ queues, exchanges, and bindings
/// </summary>
public class QueueSetup : IQueueSetup
{
    private readonly IRabbitMQConnectionFactory _connectionFactory;
    private readonly ILogger<QueueSetup> _logger;

    public QueueSetup(
        IRabbitMQConnectionFactory connectionFactory,
        ILogger<QueueSetup> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public void DeclareQueues()
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        _logger.LogInformation("Setting up RabbitMQ queues and exchanges");

        // Declare dead letter exchange
        channel.ExchangeDeclare(
            exchange: ExchangeNames.DeadLetter,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        // Declare main exchange
        channel.ExchangeDeclare(
            exchange: ExchangeNames.SlipVerification,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );

        // Declare dead letter queue
        channel.QueueDeclare(
            queue: QueueNames.DeadLetter,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Bind dead letter queue to dead letter exchange
        channel.QueueBind(
            queue: QueueNames.DeadLetter,
            exchange: ExchangeNames.DeadLetter,
            routingKey: "#"
        );

        // Queue arguments with DLQ configuration
        var queueArgs = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", ExchangeNames.DeadLetter },
            { "x-message-ttl", 3600000 }, // 1 hour
            { "x-max-length", 10000 }
        };

        // Declare slip processing queue
        channel.QueueDeclare(
            queue: QueueNames.SlipProcessing,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArgs
        );

        channel.QueueBind(
            queue: QueueNames.SlipProcessing,
            exchange: ExchangeNames.SlipVerification,
            routingKey: "slip.*"
        );

        // Declare notifications queue
        channel.QueueDeclare(
            queue: QueueNames.Notifications,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArgs
        );

        channel.QueueBind(
            queue: QueueNames.Notifications,
            exchange: ExchangeNames.SlipVerification,
            routingKey: "notification.*"
        );

        // Declare email notifications queue
        channel.QueueDeclare(
            queue: QueueNames.EmailNotifications,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArgs
        );

        channel.QueueBind(
            queue: QueueNames.EmailNotifications,
            exchange: ExchangeNames.SlipVerification,
            routingKey: RoutingKeys.NotificationEmail
        );

        // Declare push notifications queue
        channel.QueueDeclare(
            queue: QueueNames.PushNotifications,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArgs
        );

        channel.QueueBind(
            queue: QueueNames.PushNotifications,
            exchange: ExchangeNames.SlipVerification,
            routingKey: RoutingKeys.NotificationPush
        );

        // Declare reports queue
        channel.QueueDeclare(
            queue: QueueNames.Reports,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArgs
        );

        channel.QueueBind(
            queue: QueueNames.Reports,
            exchange: ExchangeNames.SlipVerification,
            routingKey: RoutingKeys.ReportGeneration
        );

        _logger.LogInformation("RabbitMQ queues and exchanges setup completed");
    }
}
