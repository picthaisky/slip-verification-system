using Moq;
using SlipVerification.Application.Features.Slips.Queries;
using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;
using SlipVerification.Domain.Interfaces;

namespace SlipVerification.UnitTests.Features.Slips.Queries;

/// <summary>
/// Unit tests for GetSlipByIdQueryHandler
/// </summary>
public class GetSlipByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Domain.Entities.SlipVerification>> _slipRepositoryMock;
    private readonly GetSlipByIdQueryHandler _handler;

    public GetSlipByIdQueryHandlerTests()
    {
        _slipRepositoryMock = new Mock<IRepository<Domain.Entities.SlipVerification>>();

        _handler = new GetSlipByIdQueryHandler(
            _slipRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidSlipId_ReturnsSuccess()
    {
        // Arrange
        var slipId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var slip = new Domain.Entities.SlipVerification
        {
            Id = slipId,
            UserId = userId,
            OrderId = orderId,
            ImagePath = "slips/test-slip.jpg",
            Status = VerificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _slipRepositoryMock
            .Setup(x => x.GetByIdAsync(slipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(slip);

        var query = new GetSlipByIdQuery { Id = slipId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(slipId, result.Data.Id);
        Assert.Equal(userId, result.Data.UserId);
        Assert.Equal(orderId, result.Data.OrderId);
        Assert.Equal("slips/test-slip.jpg", result.Data.ImagePath);

        _slipRepositoryMock.Verify(x => x.GetByIdAsync(slipId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SlipNotFound_ReturnsFailure()
    {
        // Arrange
        var slipId = Guid.NewGuid();

        _slipRepositoryMock
            .Setup(x => x.GetByIdAsync(slipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.SlipVerification?)null);

        var query = new GetSlipByIdQuery { Id = slipId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Slip not found", result.ErrorMessage);
        Assert.Null(result.Data);

        _slipRepositoryMock.Verify(x => x.GetByIdAsync(slipId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(VerificationStatus.Pending)]
    [InlineData(VerificationStatus.Verified)]
    [InlineData(VerificationStatus.Rejected)]
    public async Task Handle_DifferentStatuses_ReturnsCorrectStatus(VerificationStatus status)
    {
        // Arrange
        var slipId = Guid.NewGuid();

        var slip = new Domain.Entities.SlipVerification
        {
            Id = slipId,
            UserId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            ImagePath = "slips/test-slip.jpg",
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        _slipRepositoryMock
            .Setup(x => x.GetByIdAsync(slipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(slip);

        var query = new GetSlipByIdQuery { Id = slipId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(status.ToString(), result.Data.Status);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ThrowsException()
    {
        // Arrange
        var slipId = Guid.NewGuid();

        _slipRepositoryMock
            .Setup(x => x.GetByIdAsync(slipId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        var query = new GetSlipByIdQuery { Id = slipId };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => 
            await _handler.Handle(query, CancellationToken.None));
    }
}
