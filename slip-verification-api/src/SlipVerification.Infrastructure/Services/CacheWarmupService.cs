using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlipVerification.Application.Interfaces;
using SlipVerification.Application.Interfaces.Repositories;

namespace SlipVerification.Infrastructure.Services;

/// <summary>
/// Background service that warms up the cache on application startup
/// </summary>
public class CacheWarmupService : IHostedService
{
    private readonly ICacheService _cache;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<CacheWarmupService> _logger;

    public CacheWarmupService(
        ICacheService cache,
        IOrderRepository orderRepository,
        ILogger<CacheWarmupService> logger)
    {
        _cache = cache;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting cache warmup...");
        
        try
        {
            // Cache frequently accessed data
            var pendingOrders = await _orderRepository.GetPendingOrdersAsync(cancellationToken: cancellationToken);
            
            foreach (var order in pendingOrders)
            {
                await _cache.SetAsync(
                    $"order:{order.Id}",
                    order,
                    TimeSpan.FromMinutes(10)
                );
            }
            
            // Cache order summaries
            var summaries = await _orderRepository.GetOrderSummariesAsync(100, cancellationToken: cancellationToken);
            await _cache.SetAsync(
                "order:summaries:100:",
                summaries,
                TimeSpan.FromMinutes(5)
            );
            
            _logger.LogInformation("Cache warmup completed successfully. Cached {Count} pending orders", pendingOrders.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache warmup");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cache warmup service stopped");
        return Task.CompletedTask;
    }
}
