using Microsoft.Extensions.Logging;
using Moq;
using SlipVerification.Application.DTOs.Slips;
using SlipVerification.Application.Features.Slips.Commands;
using SlipVerification.Application.Interfaces;
using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;
using SlipVerification.Domain.Interfaces;

namespace SlipVerification.UnitTests.Features.Slips.Commands;

/// <summary>
/// Unit tests for VerifySlipCommandHandler
/// </summary>
public class VerifySlipCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepositoryMock;
    private readonly Mock<IRepository<Domain.Entities.SlipVerification>> _slipRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ILogger<VerifySlipCommandHandler>> _loggerMock;
    private readonly VerifySlipCommandHandler _handler;

    public VerifySlipCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IRepository<Order>>();
        _slipRepositoryMock = new Mock<IRepository<Domain.Entities.SlipVerification>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _loggerMock = new Mock<ILogger<VerifySlipCommandHandler>>();

        _handler = new VerifySlipCommandHandler(
            _orderRepositoryMock.Object,
            _slipRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileStorageServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var imageData = new byte[] { 0x01, 0x02, 0x03 };
        var imagePath = "slips/test-slip.jpg";

        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            OrderNumber = "ORD-001",
            Amount = 1000,
            Status = OrderStatus.PendingPayment
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imagePath);

        _slipRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.SlipVerification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.SlipVerification slip, CancellationToken ct) => slip);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new VerifySlipCommand
        {
            OrderId = orderId,
            ImageData = imageData,
            ImageFileName = "test-slip.jpg",
            ImageContentType = "image/jpeg"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(orderId, result.Data.OrderId);
        Assert.Equal(userId, result.Data.UserId);
        Assert.Equal(imagePath, result.Data.ImagePath);
        Assert.Equal(VerificationStatus.Pending.ToString(), result.Data.Status);

        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(imageData, "test-slip.jpg", "slips", It.IsAny<CancellationToken>()), Times.Once);
        _slipRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.SlipVerification>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new VerifySlipCommand
        {
            OrderId = orderId,
            ImageData = new byte[] { 0x01, 0x02, 0x03 },
            ImageFileName = "test-slip.jpg",
            ImageContentType = "image/jpeg"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Order not found", result.ErrorMessage);
        Assert.Null(result.Data);

        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _slipRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.SlipVerification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(1000, "SCB", "REF123")]
    [InlineData(5000, "KBANK", "REF456")]
    [InlineData(100.50, "BBL", "REF789")]
    public async Task Handle_ValidDataWithDifferentValues_ReturnsSuccess(decimal amount, string bankName, string refNumber)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            OrderNumber = "ORD-001",
            Amount = amount,
            Status = OrderStatus.PendingPayment
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("slips/test-slip.jpg");

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new VerifySlipCommand
        {
            OrderId = orderId,
            ImageData = new byte[] { 0x01, 0x02, 0x03 },
            ImageFileName = $"slip-{bankName}-{refNumber}.jpg",
            ImageContentType = "image/jpeg"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(orderId, result.Data.OrderId);
    }

    [Fact]
    public async Task Handle_FileStorageThrowsException_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            OrderNumber = "ORD-001",
            Amount = 1000,
            Status = OrderStatus.PendingPayment
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Disk full"));

        var command = new VerifySlipCommand
        {
            OrderId = orderId,
            ImageData = new byte[] { 0x01, 0x02, 0x03 },
            ImageFileName = "test-slip.jpg",
            ImageContentType = "image/jpeg"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("error occurred while processing", result.ErrorMessage);
    }
}
