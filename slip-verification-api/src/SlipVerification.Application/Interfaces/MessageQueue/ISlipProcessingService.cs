using SlipVerification.Application.DTOs.MessageQueue;

namespace SlipVerification.Application.Interfaces.MessageQueue;

/// <summary>
/// Service interface for processing slip verification
/// </summary>
public interface ISlipProcessingService
{
    /// <summary>
    /// Processes a slip verification request
    /// </summary>
    Task ProcessSlipAsync(SlipProcessingMessage message, CancellationToken cancellationToken);
}
