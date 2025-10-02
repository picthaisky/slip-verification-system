using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using SlipVerification.Application.Interfaces;
using SlipVerification.Infrastructure.Hubs;
using SlipVerification.Infrastructure.Services.Realtime;

namespace SlipVerification.UnitTests.Services;

/// <summary>
/// Unit tests for RealtimeNotificationService
/// </summary>
public class RealtimeNotificationServiceTests
{
    private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
    private readonly Mock<ILogger<RealtimeNotificationService>> _mockLogger;
    private readonly RealtimeNotificationService _service;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;

    public RealtimeNotificationServiceTests()
    {
        _mockHubContext = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<RealtimeNotificationService>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();

        _mockHubContext.Setup(x => x.Clients).Returns(_mockClients.Object);
        
        _service = new RealtimeNotificationService(
            _mockHubContext.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task NotifySlipVerifiedAsync_SendsToUserGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var result = new SlipVerificationResult
        {
            OrderId = Guid.NewGuid(),
            Amount = 100.00m,
            Status = "Verified"
        };

        _mockClients.Setup(x => x.Group($"user_{userId}"))
            .Returns(_mockClientProxy.Object);

        // Act
        await _service.NotifySlipVerifiedAsync(userId, result);

        // Assert
        _mockClients.Verify(x => x.Group($"user_{userId}"), Times.Once);
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "SlipVerified",
                It.Is<object[]>(o => o.Length == 1),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifyPaymentReceivedAsync_SendsToOrderGroup()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var notification = new PaymentNotification
        {
            OrderId = orderId,
            Amount = 250.50m,
            Status = "Completed",
            TransactionId = "TXN-12345",
            Timestamp = DateTime.UtcNow
        };

        _mockClients.Setup(x => x.Group($"order_{orderId}"))
            .Returns(_mockClientProxy.Object);

        // Act
        await _service.NotifyPaymentReceivedAsync(orderId, notification);

        // Assert
        _mockClients.Verify(x => x.Group($"order_{orderId}"), Times.Once);
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "PaymentReceived",
                It.Is<object[]>(o => o.Length == 1 && o[0] == notification),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task BroadcastSystemMessageAsync_SendsToAllClients()
    {
        // Arrange
        var message = "System maintenance in 30 minutes";

        _mockClients.Setup(x => x.All)
            .Returns(_mockClientProxy.Object);

        // Act
        await _service.BroadcastSystemMessageAsync(message);

        // Assert
        _mockClients.Verify(x => x.All, Times.Once);
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "SystemMessage",
                It.Is<object[]>(o => o.Length == 1),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifySlipVerifiedAsync_HandlesExceptionGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var result = new SlipVerificationResult
        {
            OrderId = Guid.NewGuid(),
            Amount = 100.00m,
            Status = "Verified"
        };

        _mockClients.Setup(x => x.Group(It.IsAny<string>()))
            .Throws(new Exception("SignalR error"));

        // Act & Assert - Should not throw
        await _service.NotifySlipVerifiedAsync(userId, result);

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifyPaymentReceivedAsync_HandlesExceptionGracefully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var notification = new PaymentNotification
        {
            OrderId = orderId,
            Amount = 250.50m,
            Status = "Completed"
        };

        _mockClients.Setup(x => x.Group(It.IsAny<string>()))
            .Throws(new Exception("SignalR error"));

        // Act & Assert - Should not throw
        await _service.NotifyPaymentReceivedAsync(orderId, notification);

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task BroadcastSystemMessageAsync_HandlesExceptionGracefully()
    {
        // Arrange
        var message = "Test message";

        _mockClients.Setup(x => x.All)
            .Throws(new Exception("SignalR error"));

        // Act & Assert - Should not throw
        await _service.BroadcastSystemMessageAsync(message);

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }
}
