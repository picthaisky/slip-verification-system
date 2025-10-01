using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlipVerification.Application.DTOs.Notifications;
using SlipVerification.Application.Interfaces.Notifications;

namespace SlipVerification.Infrastructure.Services.Notifications;

/// <summary>
/// Background service for processing notification queue
/// </summary>
public class NotificationQueueProcessor : BackgroundService, INotificationQueueService
{
    private readonly ILogger<NotificationQueueProcessor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Channel<NotificationMessage> _messageQueue;
    private readonly ConcurrentDictionary<Guid, int> _processingMessages;

    public NotificationQueueProcessor(
        ILogger<NotificationQueueProcessor> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        
        // Create an unbounded channel for the queue
        _messageQueue = Channel.CreateUnbounded<NotificationMessage>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });
        
        _processingMessages = new ConcurrentDictionary<Guid, int>();
    }

    public async Task EnqueueAsync(NotificationMessage message)
    {
        await _messageQueue.Writer.WriteAsync(message);
        _logger.LogInformation("Notification enqueued for user {UserId} on channel {Channel}", 
            message.UserId, message.Channel);
    }

    public Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        // This is handled by ExecuteAsync from BackgroundService
        return Task.CompletedTask;
    }

    public async Task ProcessNotificationAsync(NotificationMessage message)
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        try
        {
            var result = await notificationService.SendNotificationAsync(message);
            
            if (result.Success)
            {
                _logger.LogInformation("Notification {NotificationId} processed successfully for user {UserId}",
                    result.NotificationId, message.UserId);
                _processingMessages.TryRemove(message.UserId, out _);
            }
            else
            {
                _logger.LogWarning("Notification processing failed for user {UserId}: {Error}",
                    message.UserId, result.ErrorMessage);
                
                // Check if should retry
                if (_processingMessages.TryGetValue(message.UserId, out var retryCount) && retryCount < 3)
                {
                    _processingMessages[message.UserId] = retryCount + 1;
                    
                    // Re-enqueue with delay
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
                    await EnqueueAsync(message);
                }
                else
                {
                    _processingMessages.TryRemove(message.UserId, out _);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification for user {UserId}", message.UserId);
            _processingMessages.TryRemove(message.UserId, out _);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Queue Processor started");

        try
        {
            // Process messages concurrently with a degree of parallelism
            var processingTasks = new List<Task>();
            const int maxConcurrency = 10;

            for (int i = 0; i < maxConcurrency; i++)
            {
                processingTasks.Add(ProcessMessagesAsync(stoppingToken));
            }

            await Task.WhenAll(processingTasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Notification Queue Processor is stopping");
        }
    }

    private async Task ProcessMessagesAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _messageQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _processingMessages.TryAdd(message.UserId, 0);
                await ProcessNotificationAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in message processing loop");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification Queue Processor is stopping, completing pending messages");
        _messageQueue.Writer.Complete();
        await base.StopAsync(cancellationToken);
    }
}
