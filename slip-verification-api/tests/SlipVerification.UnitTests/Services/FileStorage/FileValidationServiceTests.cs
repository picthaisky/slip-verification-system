using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SlipVerification.Infrastructure.Configuration;
using SlipVerification.Infrastructure.Services.FileStorage;
using Xunit;

namespace SlipVerification.UnitTests.Services.FileStorage;

public class FileValidationServiceTests
{
    private readonly Mock<ILogger<FileValidationService>> _loggerMock;
    private readonly FileValidationOptions _options;
    private readonly FileValidationService _service;

    public FileValidationServiceTests()
    {
        _loggerMock = new Mock<ILogger<FileValidationService>>();
        _options = new FileValidationOptions
        {
            MaxFileSizeInBytes = 10_485_760, // 10MB
            EnableVirusScan = false,
            AllowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".pdf" }
        };
        _service = new FileValidationService(
            Options.Create(_options),
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task ValidateFileAsync_ValidJpegFile_ReturnsValid()
    {
        // Arrange
        var jpegHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
        using var stream = new MemoryStream(jpegHeader);
        
        // Act
        var result = await _service.ValidateFileAsync(stream, "test.jpg", "image/jpeg");
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateFileAsync_ValidPngFile_ReturnsValid()
    {
        // Arrange
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        using var stream = new MemoryStream(pngHeader);
        
        // Act
        var result = await _service.ValidateFileAsync(stream, "test.png", "image/png");
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateFileAsync_ValidPdfFile_ReturnsValid()
    {
        // Arrange
        var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };
        using var stream = new MemoryStream(pdfHeader);
        
        // Act
        var result = await _service.ValidateFileAsync(stream, "test.pdf", "application/pdf");
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateFileAsync_InvalidExtension_ReturnsInvalid()
    {
        // Arrange
        var content = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        using var stream = new MemoryStream(content);
        
        // Act
        var result = await _service.ValidateFileAsync(stream, "test.exe", "application/octet-stream");
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("not allowed"));
    }

    [Fact]
    public async Task ValidateFileAsync_EmptyFile_ReturnsInvalid()
    {
        // Arrange
        using var stream = new MemoryStream();
        
        // Act
        var result = await _service.ValidateFileAsync(stream, "test.jpg", "image/jpeg");
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("empty"));
    }

    [Fact]
    public async Task ValidateFileAsync_FileTooLarge_ReturnsInvalid()
    {
        // Arrange
        var largeContent = new byte[_options.MaxFileSizeInBytes + 1];
        using var stream = new MemoryStream(largeContent);
        
        // Act
        var result = await _service.ValidateFileAsync(stream, "test.jpg", "image/jpeg");
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("exceeds maximum"));
    }

    [Fact]
    public async Task ValidateFileAsync_WrongMagicBytes_ReturnsInvalid()
    {
        // Arrange - PNG header but .jpg extension
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        using var stream = new MemoryStream(pngHeader);
        
        // Act
        var result = await _service.ValidateFileAsync(stream, "test.jpg", "image/jpeg");
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("does not match"));
    }

    [Fact]
    public async Task ValidateFileAsync_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        using var stream = new MemoryStream(); // Empty file with invalid extension
        
        // Act
        var result = await _service.ValidateFileAsync(stream, "test.exe", "application/octet-stream");
        
        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2); // Both extension and empty errors
    }

    [Theory]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".png")]
    [InlineData(".pdf")]
    public async Task ValidateFileAsync_AllowedExtensions_AreAccepted(string extension)
    {
        // Arrange
        var validContent = extension switch
        {
            ".jpg" or ".jpeg" => new byte[] { 0xFF, 0xD8, 0xFF },
            ".png" => new byte[] { 0x89, 0x50, 0x4E, 0x47 },
            ".pdf" => new byte[] { 0x25, 0x50, 0x44, 0x46 },
            _ => throw new ArgumentException("Unsupported extension")
        };
        
        using var stream = new MemoryStream(validContent);
        
        // Act
        var result = await _service.ValidateFileAsync(stream, $"test{extension}", "application/octet-stream");
        
        // Assert
        Assert.True(result.IsValid);
    }
}
