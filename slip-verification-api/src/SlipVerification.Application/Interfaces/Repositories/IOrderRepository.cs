using SlipVerification.Application.DTOs;
using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Order entity with performance-optimized methods
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Gets an order by ID with related entities (using compiled query)
    /// </summary>
    Task<Order?> GetByIdWithIncludesAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets pending orders (read-only, optimized)
    /// </summary>
    Task<IEnumerable<Order>> GetPendingOrdersAsync(int limit = 100, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets orders with pagination
    /// </summary>
    Task<PagedResult<Order>> GetOrdersPagedAsync(
        int page, 
        int pageSize, 
        OrderStatus? status = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets order summaries (projection, read-only)
    /// </summary>
    Task<IEnumerable<OrderSummaryDto>> GetOrderSummariesAsync(
        int limit = 100,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets order detail with split queries for better performance
    /// </summary>
    Task<OrderDetailDto?> GetOrderDetailAsync(Guid orderId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Streams orders using IAsyncEnumerable for large datasets
    /// </summary>
    IAsyncEnumerable<Order> StreamOrdersAsync(
        OrderStatus? status = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets orders by user ID (read-only)
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(
        Guid userId,
        int limit = 100,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new order
    /// </summary>
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing order
    /// </summary>
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets order by order number
    /// </summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
}
