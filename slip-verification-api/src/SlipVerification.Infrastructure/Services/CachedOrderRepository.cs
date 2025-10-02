using SlipVerification.Application.DTOs;
using SlipVerification.Application.Interfaces;
using SlipVerification.Application.Interfaces.Repositories;
using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Infrastructure.Services;

/// <summary>
/// Caching decorator for OrderRepository using decorator pattern
/// </summary>
public class CachedOrderRepository : IOrderRepository
{
    private readonly IOrderRepository _innerRepository;
    private readonly ICacheService _cache;
    
    // Cache key prefixes
    private const string OrderCachePrefix = "order:";
    private const string OrderSummariesCacheKey = "order:summaries";
    private const string PendingOrdersCacheKey = "order:pending";
    
    public CachedOrderRepository(IOrderRepository innerRepository, ICacheService cache)
    {
        _innerRepository = innerRepository;
        _cache = cache;
    }

    public async Task<Order?> GetByIdWithIncludesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{OrderCachePrefix}{id}";
        
        return await _cache.GetOrSetAsync(
            cacheKey,
            () => _innerRepository.GetByIdWithIncludesAsync(id, cancellationToken),
            TimeSpan.FromMinutes(5)
        );
    }

    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{PendingOrdersCacheKey}:{limit}";
        
        return await _cache.GetOrSetAsync(
            cacheKey,
            () => _innerRepository.GetPendingOrdersAsync(limit, cancellationToken),
            TimeSpan.FromMinutes(1)
        ) ?? Enumerable.Empty<Order>();
    }

    public Task<PagedResult<Order>> GetOrdersPagedAsync(
        int page, 
        int pageSize, 
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        // Don't cache paginated results as they change frequently
        return _innerRepository.GetOrdersPagedAsync(page, pageSize, status, cancellationToken);
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetOrderSummariesAsync(
        int limit = 100,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{OrderSummariesCacheKey}:{limit}:{status}";
        
        return await _cache.GetOrSetAsync(
            cacheKey,
            () => _innerRepository.GetOrderSummariesAsync(limit, status, cancellationToken),
            TimeSpan.FromMinutes(2)
        ) ?? Enumerable.Empty<OrderSummaryDto>();
    }

    public async Task<OrderDetailDto?> GetOrderDetailAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{OrderCachePrefix}detail:{orderId}";
        
        return await _cache.GetOrSetAsync(
            cacheKey,
            () => _innerRepository.GetOrderDetailAsync(orderId, cancellationToken),
            TimeSpan.FromMinutes(5)
        );
    }

    public IAsyncEnumerable<Order> StreamOrdersAsync(
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        // Don't cache streaming operations
        return _innerRepository.StreamOrdersAsync(status, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(
        Guid userId,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{OrderCachePrefix}user:{userId}:{limit}";
        
        return await _cache.GetOrSetAsync(
            cacheKey,
            () => _innerRepository.GetOrdersByUserIdAsync(userId, limit, cancellationToken),
            TimeSpan.FromMinutes(3)
        ) ?? Enumerable.Empty<Order>();
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        var result = await _innerRepository.AddAsync(order, cancellationToken);
        
        // Invalidate relevant caches
        await _cache.RemoveByPrefixAsync(OrderCachePrefix);
        await _cache.RemoveAsync(PendingOrdersCacheKey);
        await _cache.RemoveAsync(OrderSummariesCacheKey);
        
        return result;
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _innerRepository.UpdateAsync(order, cancellationToken);
        
        // Invalidate cache for this specific order and related caches
        await _cache.RemoveAsync($"{OrderCachePrefix}{order.Id}");
        await _cache.RemoveAsync($"{OrderCachePrefix}detail:{order.Id}");
        await _cache.RemoveAsync($"{OrderCachePrefix}user:{order.UserId}");
        await _cache.RemoveAsync(PendingOrdersCacheKey);
        await _cache.RemoveByPrefixAsync(OrderSummariesCacheKey);
    }

    public Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        // Don't cache this as it's less frequently used
        return _innerRepository.GetByOrderNumberAsync(orderNumber, cancellationToken);
    }
}
