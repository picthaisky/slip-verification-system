using SlipVerification.Application.DTOs;
using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for SlipVerification entity with performance-optimized methods
/// </summary>
public interface ISlipVerificationRepository
{
    /// <summary>
    /// Gets a slip verification by ID (read-only)
    /// </summary>
    Task<SlipVerification.Domain.Entities.SlipVerification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets slip verifications with pagination
    /// </summary>
    Task<PagedResult<SlipVerification.Domain.Entities.SlipVerification>> GetPagedAsync(
        int page,
        int pageSize,
        VerificationStatus? status = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets slip verifications by order ID (read-only)
    /// </summary>
    Task<IEnumerable<SlipVerification.Domain.Entities.SlipVerification>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets slip verifications by user ID (read-only)
    /// </summary>
    Task<IEnumerable<SlipVerification.Domain.Entities.SlipVerification>> GetByUserIdAsync(
        Guid userId,
        int limit = 100,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a slip with the same image hash already exists
    /// </summary>
    Task<bool> ExistsByImageHashAsync(string imageHash, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Streams slip verifications for batch processing
    /// </summary>
    IAsyncEnumerable<SlipVerification.Domain.Entities.SlipVerification> StreamByStatusAsync(
        VerificationStatus status,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new slip verification
    /// </summary>
    Task<SlipVerification.Domain.Entities.SlipVerification> AddAsync(
        SlipVerification.Domain.Entities.SlipVerification slip,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing slip verification
    /// </summary>
    Task UpdateAsync(
        SlipVerification.Domain.Entities.SlipVerification slip,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Processes multiple slips in parallel with controlled concurrency
    /// </summary>
    Task ProcessSlipsBatchAsync(
        IEnumerable<Guid> slipIds,
        Func<Guid, Task> processFunc,
        int maxConcurrency = 10,
        CancellationToken cancellationToken = default);
}
