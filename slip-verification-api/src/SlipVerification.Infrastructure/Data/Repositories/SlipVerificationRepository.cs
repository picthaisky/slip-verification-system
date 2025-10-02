using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using SlipVerification.Application.DTOs;
using SlipVerification.Application.Interfaces.Repositories;
using SlipVerification.Domain.Enums;
using SlipVerification.Infrastructure.Data;

namespace SlipVerification.Infrastructure.Data.Repositories;

/// <summary>
/// Performance-optimized repository for SlipVerification entity
/// </summary>
public class SlipVerificationRepository : ISlipVerificationRepository
{
    private readonly ApplicationDbContext _context;

    public SlipVerificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SlipVerification.Domain.Entities.SlipVerification?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.SlipVerifications
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<PagedResult<SlipVerification.Domain.Entities.SlipVerification>> GetPagedAsync(
        int page,
        int pageSize,
        VerificationStatus? status = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SlipVerifications
            .AsNoTracking()
            .Where(s => !s.IsDeleted);
        
        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }
        
        if (userId.HasValue)
        {
            query = query.Where(s => s.UserId == userId.Value);
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return new PagedResult<SlipVerification.Domain.Entities.SlipVerification>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<SlipVerification.Domain.Entities.SlipVerification>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.SlipVerifications
            .AsNoTracking()
            .Where(s => s.OrderId == orderId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SlipVerification.Domain.Entities.SlipVerification>> GetByUserIdAsync(
        Guid userId,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        return await _context.SlipVerifications
            .AsNoTracking()
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByImageHashAsync(string imageHash, CancellationToken cancellationToken = default)
    {
        return await _context.SlipVerifications
            .AsNoTracking()
            .AnyAsync(s => s.ImageHash == imageHash && !s.IsDeleted, cancellationToken);
    }

    public async IAsyncEnumerable<SlipVerification.Domain.Entities.SlipVerification> StreamByStatusAsync(
        VerificationStatus status,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _context.SlipVerifications
            .AsNoTracking()
            .Where(s => s.Status == status && !s.IsDeleted)
            .AsAsyncEnumerable();
        
        await foreach (var slip in query.WithCancellation(cancellationToken))
        {
            yield return slip;
        }
    }

    public async Task<SlipVerification.Domain.Entities.SlipVerification> AddAsync(
        SlipVerification.Domain.Entities.SlipVerification slip,
        CancellationToken cancellationToken = default)
    {
        await _context.SlipVerifications.AddAsync(slip, cancellationToken);
        return slip;
    }

    public Task UpdateAsync(
        SlipVerification.Domain.Entities.SlipVerification slip,
        CancellationToken cancellationToken = default)
    {
        _context.SlipVerifications.Update(slip);
        return Task.CompletedTask;
    }

    public async Task ProcessSlipsBatchAsync(
        IEnumerable<Guid> slipIds,
        Func<Guid, Task> processFunc,
        int maxConcurrency = 10,
        CancellationToken cancellationToken = default)
    {
        // Parallel processing with SemaphoreSlim to control concurrency
        var semaphore = new SemaphoreSlim(maxConcurrency);
        var tasks = new List<Task>();
        
        foreach (var slipId in slipIds)
        {
            await semaphore.WaitAsync(cancellationToken);
            
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await processFunc(slipId);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }
        
        await Task.WhenAll(tasks);
    }
}
