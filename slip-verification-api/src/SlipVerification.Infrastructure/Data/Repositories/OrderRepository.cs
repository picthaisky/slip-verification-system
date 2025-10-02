using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using SlipVerification.Application.DTOs;
using SlipVerification.Application.Interfaces.Repositories;
using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;
using SlipVerification.Infrastructure.Data;

namespace SlipVerification.Infrastructure.Data.Repositories;

/// <summary>
/// Performance-optimized repository for Order entity
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    
    // Compiled query for frequently executed query - GetById with includes
    private static readonly Func<ApplicationDbContext, Guid, Task<Order?>> GetOrderByIdQuery =
        EF.CompileAsyncQuery((ApplicationDbContext context, Guid id) =>
            context.Orders
                .Include(o => o.SlipVerifications)
                .Include(o => o.User)
                .Include(o => o.Transactions)
                .FirstOrDefault(o => o.Id == id && !o.IsDeleted)
        );

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdWithIncludesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Use compiled query for best performance
        return await GetOrderByIdQuery(_context, id);
    }

    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        // Use AsNoTracking for read-only queries
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.PendingPayment && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Order>> GetOrdersPagedAsync(
        int page, 
        int pageSize, 
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => !o.IsDeleted);
        
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetOrderSummariesAsync(
        int limit = 100,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        // Use projection (Select) to fetch only needed columns
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => !o.IsDeleted);
        
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }
        
        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Amount = o.Amount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt,
                Description = o.Description
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderDetailDto?> GetOrderDetailAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // Split large queries for better performance
        
        // First query: Get order
        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);
        
        if (order == null) return null;
        
        // Second query: Get slip verifications
        var slips = await _context.SlipVerifications
            .AsNoTracking()
            .Where(s => s.OrderId == orderId && !s.IsDeleted)
            .ToListAsync(cancellationToken);
        
        // Third query: Get transactions
        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.OrderId == orderId && !t.IsDeleted)
            .ToListAsync(cancellationToken);
        
        return new OrderDetailDto
        {
            Order = order,
            Slips = slips,
            Transactions = transactions
        };
    }

    public async IAsyncEnumerable<Order> StreamOrdersAsync(
        OrderStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Use IAsyncEnumerable for streaming large datasets
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => !o.IsDeleted);
        
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }
        
        await foreach (var order in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return order;
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(
        Guid userId,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        return order;
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        return Task.CompletedTask;
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && !o.IsDeleted, cancellationToken);
    }
}
