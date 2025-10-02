using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipVerification.Application.DTOs.MessageQueue;
using SlipVerification.Application.DTOs.Notifications;
using SlipVerification.Application.Interfaces.MessageQueue;
using SlipVerification.Application.Interfaces.Notifications;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Infrastructure.MessageQueue.Consumers;

/// <summary>
/// Consumer for processing notification messages
/// </summary>
public class NotificationConsumer : BaseRabbitMQConsumer
{
    public NotificationConsumer(
        IRabbitMQConnectionFactory connectionFactory,
        IServiceProvider serviceProvider,
        ILogger<NotificationConsumer> logger)
        : base(connectionFactory, serviceProvider, logger)
    {
    }

    protected override string QueueName => QueueNames.Notifications;

    protected override async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        using var scope = ServiceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var notificationMessage = JsonSerializer.Deserialize<NotificationQueueMessage>(message);
        if (notificationMessage == null)
        {
            throw new InvalidOperationException("Failed to deserialize notification message");
        }

        // Convert to NotificationMessage
        var notification = new NotificationMessage
        {
            UserId = notificationMessage.UserId,
            Channel = Enum.Parse<NotificationChannel>(notificationMessage.Channel),
            Title = notificationMessage.Title,
            Message = notificationMessage.Message,
            Priority = (NotificationPriority)notificationMessage.Priority,
            Data = notificationMessage.Data
        };

        await notificationService.SendNotificationAsync(notification);
    }
}
