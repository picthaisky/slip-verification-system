using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SlipVerification.Application.DTOs.Notifications;
using SlipVerification.Domain.Enums;
using SlipVerification.Infrastructure.Services.Notifications;
using Xunit;

namespace SlipVerification.UnitTests.Services.Notifications;

/// <summary>
/// Unit tests for NotificationChannels
/// </summary>
public class NotificationChannelTests
{
    [Fact]
    public void LineNotifyChannel_SupportsChannel_ReturnsTrue_ForLineChannel()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LineNotifyChannel>>();
        var mockOptions = new Mock<IOptions<LineNotifyOptions>>();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        
        mockOptions.Setup(o => o.Value).Returns(new LineNotifyOptions());
        mockHttpClientFactory.Setup(f => f.CreateClient("LineNotify"))
            .Returns(new HttpClient());

        var channel = new LineNotifyChannel(
            mockLogger.Object,
            mockOptions.Object,
            mockHttpClientFactory.Object);

        // Act
        var result = channel.SupportsChannel(NotificationChannel.LINE);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void LineNotifyChannel_SupportsChannel_ReturnsFalse_ForOtherChannels()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LineNotifyChannel>>();
        var mockOptions = new Mock<IOptions<LineNotifyOptions>>();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        
        mockOptions.Setup(o => o.Value).Returns(new LineNotifyOptions());
        mockHttpClientFactory.Setup(f => f.CreateClient("LineNotify"))
            .Returns(new HttpClient());

        var channel = new LineNotifyChannel(
            mockLogger.Object,
            mockOptions.Object,
            mockHttpClientFactory.Object);

        // Act & Assert
        Assert.False(channel.SupportsChannel(NotificationChannel.EMAIL));
        Assert.False(channel.SupportsChannel(NotificationChannel.PUSH));
        Assert.False(channel.SupportsChannel(NotificationChannel.SMS));
    }

    [Fact]
    public void EmailChannel_SupportsChannel_ReturnsTrue_ForEmailChannel()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailChannel>>();
        var mockOptions = new Mock<IOptions<EmailOptions>>();
        
        mockOptions.Setup(o => o.Value).Returns(new EmailOptions
        {
            SmtpHost = "smtp.test.com",
            FromEmail = "test@test.com"
        });

        var channel = new EmailChannel(mockLogger.Object, mockOptions.Object);

        // Act
        var result = channel.SupportsChannel(NotificationChannel.EMAIL);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task LineNotifyChannel_SendAsync_ReturnsFailure_WhenTokenMissing()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LineNotifyChannel>>();
        var mockOptions = new Mock<IOptions<LineNotifyOptions>>();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        
        mockOptions.Setup(o => o.Value).Returns(new LineNotifyOptions());
        mockHttpClientFactory.Setup(f => f.CreateClient("LineNotify"))
            .Returns(new HttpClient());

        var channel = new LineNotifyChannel(
            mockLogger.Object,
            mockOptions.Object,
            mockHttpClientFactory.Object);

        var message = new NotificationMessage
        {
            UserId = Guid.NewGuid(),
            Channel = NotificationChannel.LINE,
            Title = "Test",
            Message = "Test message",
            LineToken = null // Missing token
        };

        // Act
        var result = await channel.SendAsync(message);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("token", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EmailChannel_SendAsync_ReturnsFailure_WhenRecipientMissing()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailChannel>>();
        var mockOptions = new Mock<IOptions<EmailOptions>>();
        
        mockOptions.Setup(o => o.Value).Returns(new EmailOptions
        {
            SmtpHost = "smtp.test.com",
            FromEmail = "test@test.com"
        });

        var channel = new EmailChannel(mockLogger.Object, mockOptions.Object);

        var message = new NotificationMessage
        {
            UserId = Guid.NewGuid(),
            Channel = NotificationChannel.EMAIL,
            Title = "Test",
            Message = "Test message",
            RecipientEmail = null // Missing recipient
        };

        // Act
        var result = await channel.SendAsync(message);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("email", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NotificationChannel_ChannelType_ReturnsCorrectType()
    {
        // Arrange & Act
        var mockLogger = new Mock<ILogger<LineNotifyChannel>>();
        var mockOptions = new Mock<IOptions<LineNotifyOptions>>();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        
        mockOptions.Setup(o => o.Value).Returns(new LineNotifyOptions());
        mockHttpClientFactory.Setup(f => f.CreateClient("LineNotify"))
            .Returns(new HttpClient());

        var lineChannel = new LineNotifyChannel(
            mockLogger.Object,
            mockOptions.Object,
            mockHttpClientFactory.Object);

        var emailMockLogger = new Mock<ILogger<EmailChannel>>();
        var emailMockOptions = new Mock<IOptions<EmailOptions>>();
        emailMockOptions.Setup(o => o.Value).Returns(new EmailOptions());

        var emailChannel = new EmailChannel(emailMockLogger.Object, emailMockOptions.Object);

        // Assert
        Assert.Equal(NotificationChannel.LINE, lineChannel.ChannelType);
        Assert.Equal(NotificationChannel.EMAIL, emailChannel.ChannelType);
    }
}
