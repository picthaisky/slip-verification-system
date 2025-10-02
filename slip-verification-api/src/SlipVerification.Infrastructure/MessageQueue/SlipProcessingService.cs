using Microsoft.Extensions.Logging;
using SlipVerification.Application.DTOs.MessageQueue;
using SlipVerification.Application.Interfaces.MessageQueue;

namespace SlipVerification.Infrastructure.MessageQueue;

/// <summary>
/// Service for processing slip verification (stub implementation)
/// </summary>
public class SlipProcessingService : ISlipProcessingService
{
    private readonly ILogger<SlipProcessingService> _logger;

    public SlipProcessingService(ILogger<SlipProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessSlipAsync(SlipProcessingMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing slip {SlipId} for user {UserId}. Image URL: {ImageUrl}, Type: {ProcessingType}",
            message.SlipId,
            message.UserId,
            message.ImageUrl,
            message.ProcessingType
        );

        // Simulate OCR processing
        await Task.Delay(2000, cancellationToken);

        // TODO: Implement actual OCR processing logic
        // - Call OCR service
        // - Extract slip data
        // - Validate slip information
        // - Update database
        // - Send notifications

        _logger.LogInformation("Slip {SlipId} processed successfully", message.SlipId);
    }
}
