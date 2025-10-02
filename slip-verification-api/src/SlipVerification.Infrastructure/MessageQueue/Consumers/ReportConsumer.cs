using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipVerification.Application.DTOs.MessageQueue;
using SlipVerification.Application.Interfaces.MessageQueue;

namespace SlipVerification.Infrastructure.MessageQueue.Consumers;

/// <summary>
/// Consumer for processing report generation messages
/// </summary>
public class ReportConsumer : BaseRabbitMQConsumer
{
    public ReportConsumer(
        IRabbitMQConnectionFactory connectionFactory,
        IServiceProvider serviceProvider,
        ILogger<ReportConsumer> logger)
        : base(connectionFactory, serviceProvider, logger)
    {
    }

    protected override string QueueName => QueueNames.Reports;

    protected override async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        var reportMessage = JsonSerializer.Deserialize<ReportGenerationMessage>(message);
        if (reportMessage == null)
        {
            throw new InvalidOperationException("Failed to deserialize report generation message");
        }

        Logger.LogInformation(
            "Processing report {ReportId} of type {ReportType} for user {UserId}",
            reportMessage.ReportId,
            reportMessage.ReportType,
            reportMessage.UserId
        );

        // TODO: Implement actual report generation logic
        // This is a placeholder for the report service
        await Task.Delay(1000, cancellationToken); // Simulate work

        Logger.LogInformation("Report {ReportId} generated successfully", reportMessage.ReportId);
    }
}
