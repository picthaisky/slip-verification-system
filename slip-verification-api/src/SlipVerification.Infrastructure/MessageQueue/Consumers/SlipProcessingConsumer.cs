using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipVerification.Application.DTOs.MessageQueue;
using SlipVerification.Application.Interfaces.MessageQueue;

namespace SlipVerification.Infrastructure.MessageQueue.Consumers;

/// <summary>
/// Consumer for processing slip verification messages
/// </summary>
public class SlipProcessingConsumer : BaseRabbitMQConsumer
{
    public SlipProcessingConsumer(
        IRabbitMQConnectionFactory connectionFactory,
        IServiceProvider serviceProvider,
        ILogger<SlipProcessingConsumer> logger)
        : base(connectionFactory, serviceProvider, logger)
    {
    }

    protected override string QueueName => QueueNames.SlipProcessing;

    protected override async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        using var scope = ServiceProvider.CreateScope();
        var slipService = scope.ServiceProvider.GetRequiredService<ISlipProcessingService>();

        var slipMessage = JsonSerializer.Deserialize<SlipProcessingMessage>(message);
        if (slipMessage == null)
        {
            throw new InvalidOperationException("Failed to deserialize slip processing message");
        }

        await slipService.ProcessSlipAsync(slipMessage, cancellationToken);
    }
}
