using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SlipVerification.Application.Interfaces.MessageQueue;

namespace SlipVerification.Infrastructure.MessageQueue;

/// <summary>
/// Base class for RabbitMQ consumers with retry mechanism and DLQ handling
/// </summary>
public abstract class BaseRabbitMQConsumer : BackgroundService
{
    protected readonly IRabbitMQConnectionFactory ConnectionFactory;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ILogger Logger;
    protected IModel? Channel;
    protected IConnection? Connection;

    protected abstract string QueueName { get; }
    protected virtual int PrefetchCount => 10;
    protected virtual int MaxRetryCount => 3;

    protected BaseRabbitMQConsumer(
        IRabbitMQConnectionFactory connectionFactory,
        IServiceProvider serviceProvider,
        ILogger logger)
    {
        ConnectionFactory = connectionFactory;
        ServiceProvider = serviceProvider;
        Logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Connection = ConnectionFactory.CreateConnection();
        Channel = Connection.CreateModel();

        Channel.BasicQos(prefetchSize: 0, prefetchCount: (ushort)PrefetchCount, global: false);

        var consumer = new EventingBasicConsumer(Channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                Logger.LogInformation("Processing message from queue {QueueName}", QueueName);

                await ProcessMessageAsync(message, stoppingToken);

                Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                Logger.LogInformation("Message processed successfully from queue {QueueName}", QueueName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing message from queue {QueueName}", QueueName);

                // Handle retry logic
                await HandleRetryAsync(ea, body, ex);
            }
        };

        Channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer
        );

        Logger.LogInformation("Consumer started for queue {QueueName}", QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    protected abstract Task ProcessMessageAsync(string message, CancellationToken cancellationToken);

    private async Task HandleRetryAsync(BasicDeliverEventArgs ea, byte[] body, Exception ex)
    {
        var retryCount = GetRetryCount(ea.BasicProperties);

        if (retryCount < MaxRetryCount)
        {
            // Calculate exponential backoff delay
            var delaySeconds = (int)Math.Pow(2, retryCount);

            Logger.LogWarning(
                "Retrying message (attempt {RetryCount}/{MaxRetryCount}) after {DelaySeconds}s delay. Error: {Error}",
                retryCount + 1,
                MaxRetryCount,
                delaySeconds,
                ex.Message
            );

            // Requeue with delay and incremented retry count
            var properties = Channel!.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = ea.BasicProperties.ContentType;
            properties.MessageId = ea.BasicProperties.MessageId;
            
            if (properties.Headers == null)
            {
                properties.Headers = new Dictionary<string, object>();
            }
            properties.Headers["x-retry-count"] = retryCount + 1;
            properties.Headers["x-first-death-reason"] = ex.Message;

            // Delay before requeue
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

            Channel.BasicPublish("", ea.RoutingKey, properties, body);
            Channel.BasicAck(ea.DeliveryTag, false);
        }
        else
        {
            Logger.LogError(
                "Max retry count reached for message. Sending to DLQ. Error: {Error}",
                ex.Message
            );

            // Send to dead letter queue by rejecting
            Channel!.BasicNack(ea.DeliveryTag, false, false);
        }
    }

    private int GetRetryCount(IBasicProperties properties)
    {
        if (properties.Headers?.ContainsKey("x-retry-count") == true)
        {
            var value = properties.Headers["x-retry-count"];
            if (value is int intValue)
            {
                return intValue;
            }
            if (value is byte[] bytes && bytes.Length == 4)
            {
                return BitConverter.ToInt32(bytes, 0);
            }
        }
        return 0;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Consumer stopping for queue {QueueName}", QueueName);

        if (Channel != null && Channel.IsOpen)
        {
            Channel.Close();
            Channel.Dispose();
        }

        if (Connection != null && Connection.IsOpen)
        {
            Connection.Close();
            Connection.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}
