using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipVerification.Application.Interfaces.MessageQueue;
using SlipVerification.Infrastructure.MessageQueue;
using SlipVerification.Infrastructure.MessageQueue.Consumers;

namespace SlipVerification.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring message queue services
/// </summary>
public static class MessageQueueServiceExtensions
{
    /// <summary>
    /// Adds RabbitMQ message queue services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddMessageQueueServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure RabbitMQ options
        services.Configure<RabbitMQConfiguration>(
            configuration.GetSection(RabbitMQConfiguration.SectionName));

        // Register connection factory as singleton
        services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();

        // Register queue setup
        services.AddSingleton<IQueueSetup, QueueSetup>();

        // Register message publisher
        services.AddScoped<IMessagePublisher, RabbitMQPublisher>();

        // Register processing services
        services.AddScoped<ISlipProcessingService, SlipProcessingService>();

        // Register health check
        services.AddHealthChecks()
            .AddCheck<RabbitMQHealthCheck>("rabbitmq", tags: new[] { "ready", "messaging" });

        // Register consumers as hosted services
        services.AddHostedService<SlipProcessingConsumer>();
        services.AddHostedService<NotificationConsumer>();
        services.AddHostedService<ReportConsumer>();

        return services;
    }

    /// <summary>
    /// Initializes RabbitMQ queues on application startup
    /// </summary>
    public static IServiceProvider InitializeMessageQueues(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var queueSetup = scope.ServiceProvider.GetRequiredService<IQueueSetup>();
        
        try
        {
            queueSetup.DeclareQueues();
        }
        catch (Exception ex)
        {
            // Log error but don't fail application startup
            var logger = scope.ServiceProvider.GetService<ILogger<QueueSetup>>();
            logger?.LogError(ex, "Failed to initialize message queues. RabbitMQ might not be available.");
        }

        return serviceProvider;
    }
}
