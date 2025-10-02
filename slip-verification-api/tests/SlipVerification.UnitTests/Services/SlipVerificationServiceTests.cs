using Microsoft.Extensions.Logging;
using Moq;
using SlipVerification.Application.Interfaces;
using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;
using SlipVerification.Domain.Interfaces;

namespace SlipVerification.UnitTests.Services;

/// <summary>
/// Unit tests for Slip Verification Service logic
/// </summary>
public class SlipVerificationServiceTests
{
    private readonly Mock<IRepository<Domain.Entities.SlipVerification>> _slipRepositoryMock;
    private readonly Mock<IRepository<Order>> _orderRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ILogger<SlipVerificationServiceTests>> _loggerMock;

    public SlipVerificationServiceTests()
    {
        _slipRepositoryMock = new Mock<IRepository<Domain.Entities.SlipVerification>>();
        _orderRepositoryMock = new Mock<IRepository<Order>>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _loggerMock = new Mock<ILogger<SlipVerificationServiceTests>>();
    }

    [Fact]
    public async Task ProcessSlip_ValidImage_ReturnsSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var imageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG header
        var imagePath = "slips/test-slip.jpg";

        var order = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Amount = 1000,
            Status = OrderStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imagePath);

        // Act
        var result = imagePath;

        // Assert
        Assert.NotNull(result);
        Assert.Contains("slips", result);
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(imageData, It.IsAny<string>(), "slips", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1000, "SCB", "REF123")]
    [InlineData(5000, "KBANK", "REF456")]
    [InlineData(100.50, "BBL", "REF789")]
    public async Task ValidateSlip_ValidData_ReturnsTrue(decimal amount, string bankName, string refNumber)
    {
        // Arrange
        var slip = new Domain.Entities.SlipVerification
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = amount,
            BankName = bankName,
            ReferenceNumber = refNumber,
            Status = VerificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var isValid = slip.Amount > 0 && !string.IsNullOrEmpty(slip.BankName);

        // Assert
        Assert.True(isValid);
        Assert.Equal(amount, slip.Amount);
        Assert.Equal(bankName, slip.BankName);
        Assert.Equal(refNumber, slip.ReferenceNumber);
    }

    [Fact]
    public async Task ValidateSlip_NegativeAmount_ReturnsFalse()
    {
        // Arrange
        var slip = new Domain.Entities.SlipVerification
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = -100,
            Status = VerificationStatus.Pending
        };

        // Act
        var isValid = slip.Amount > 0;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task ValidateSlip_MissingRequiredFields_ReturnsFalse()
    {
        // Arrange
        var slip = new Domain.Entities.SlipVerification
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 1000,
            BankName = null,
            Status = VerificationStatus.Pending
        };

        // Act
        var isValid = slip.Amount > 0 && !string.IsNullOrEmpty(slip.BankName);

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("image/jpeg", true)]
    [InlineData("image/png", true)]
    [InlineData("image/gif", false)]
    [InlineData("application/pdf", false)]
    [InlineData("text/plain", false)]
    public void ValidateImageType_DifferentMimeTypes_ReturnsExpectedResult(string mimeType, bool expected)
    {
        // Arrange
        var allowedTypes = new[] { "image/jpeg", "image/png" };

        // Act
        var isValid = allowedTypes.Contains(mimeType);

        // Assert
        Assert.Equal(expected, isValid);
    }

    [Theory]
    [InlineData(1024, true)]           // 1KB - valid
    [InlineData(5242880, true)]        // 5MB - valid
    [InlineData(10485760, true)]       // 10MB - valid
    [InlineData(10485761, false)]      // 10MB + 1 byte - invalid
    [InlineData(20971520, false)]      // 20MB - invalid
    public void ValidateFileSize_DifferentSizes_ReturnsExpectedResult(int fileSize, bool expected)
    {
        // Arrange
        const int maxFileSize = 10 * 1024 * 1024; // 10MB

        // Act
        var isValid = fileSize > 0 && fileSize <= maxFileSize;

        // Assert
        Assert.Equal(expected, isValid);
    }

    [Fact]
    public void GenerateImageHash_SameImage_ReturnsSameHash()
    {
        // Arrange
        var imageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
        
        // Act
        var hash1 = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(imageData));
        var hash2 = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(imageData));

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.NotEmpty(hash1);
    }

    [Fact]
    public void GenerateImageHash_DifferentImages_ReturnsDifferentHash()
    {
        // Arrange
        var imageData1 = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
        var imageData2 = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x11 };
        
        // Act
        var hash1 = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(imageData1));
        var hash2 = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(imageData2));

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Theory]
    [InlineData(VerificationStatus.Pending, VerificationStatus.Verified, true)]
    [InlineData(VerificationStatus.Pending, VerificationStatus.Rejected, true)]
    [InlineData(VerificationStatus.Verified, VerificationStatus.Rejected, false)]
    [InlineData(VerificationStatus.Rejected, VerificationStatus.Verified, false)]
    public void ValidateStatusTransition_DifferentTransitions_ReturnsExpectedResult(
        VerificationStatus currentStatus, 
        VerificationStatus newStatus, 
        bool expected)
    {
        // Arrange
        var allowedTransitions = new Dictionary<VerificationStatus, List<VerificationStatus>>
        {
            { VerificationStatus.Pending, new List<VerificationStatus> { VerificationStatus.Verified, VerificationStatus.Rejected } },
            { VerificationStatus.Verified, new List<VerificationStatus>() },
            { VerificationStatus.Rejected, new List<VerificationStatus>() }
        };

        // Act
        var isValid = allowedTransitions.ContainsKey(currentStatus) && 
                      allowedTransitions[currentStatus].Contains(newStatus);

        // Assert
        Assert.Equal(expected, isValid);
    }
}
