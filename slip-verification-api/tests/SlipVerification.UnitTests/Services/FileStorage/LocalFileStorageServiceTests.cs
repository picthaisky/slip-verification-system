using SlipVerification.Infrastructure.Services;
using Xunit;

namespace SlipVerification.UnitTests.Services.FileStorage;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly string _testBaseUrl;
    private readonly LocalFileStorageService _service;

    public LocalFileStorageServiceTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), $"file-storage-test-{Guid.NewGuid()}");
        _testBaseUrl = "http://localhost:5000/uploads";
        _service = new LocalFileStorageService(_testBasePath, _testBaseUrl);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, true);
        }
    }

    [Fact]
    public async Task UploadFileAsync_ValidFile_ReturnsUploadResult()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3, 4, 5 };
        using var stream = new MemoryStream(content);
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var metadata = new Dictionary<string, string> { ["user"] = "test-user" };

        // Act
        var result = await _service.UploadFileAsync(stream, fileName, contentType, metadata);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.FileKey);
        Assert.Equal(fileName, result.FileName);
        Assert.Equal(contentType, result.ContentType);
        Assert.Equal(content.Length, result.SizeInBytes); // The file should have the correct size on disk
        Assert.Contains(_testBaseUrl, result.Url);
        Assert.NotNull(result.ETag);
        
        // Verify file was actually written
        var exists = await _service.FileExistsAsync(result.FileKey);
        Assert.True(exists);
    }

    [Fact]
    public async Task DownloadFileAsync_ExistingFile_ReturnsStream()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3, 4, 5 };
        using var uploadStream = new MemoryStream(content);
        var uploadResult = await _service.UploadFileAsync(uploadStream, "test.jpg", "image/jpeg");

        // Act
        using var downloadStream = await _service.DownloadFileAsync(uploadResult.FileKey);

        // Assert
        Assert.NotNull(downloadStream);
        var downloadedContent = new byte[content.Length];
        await downloadStream.ReadAsync(downloadedContent, 0, downloadedContent.Length);
        Assert.Equal(content, downloadedContent);
    }

    [Fact]
    public async Task DownloadFileAsync_NonExistingFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistingKey = "non-existing/file.jpg";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => 
            _service.DownloadFileAsync(nonExistingKey));
    }

    [Fact]
    public async Task FileExistsAsync_ExistingFile_ReturnsTrue()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3, 4, 5 };
        using var stream = new MemoryStream(content);
        var uploadResult = await _service.UploadFileAsync(stream, "test.jpg", "image/jpeg");

        // Act
        var exists = await _service.FileExistsAsync(uploadResult.FileKey);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task FileExistsAsync_NonExistingFile_ReturnsFalse()
    {
        // Arrange
        var nonExistingKey = "non-existing/file.jpg";

        // Act
        var exists = await _service.FileExistsAsync(nonExistingKey);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteFileAsync_ExistingFile_ReturnsTrue()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3, 4, 5 };
        using var stream = new MemoryStream(content);
        var uploadResult = await _service.UploadFileAsync(stream, "test.jpg", "image/jpeg");

        // Act
        var deleted = await _service.DeleteFileAsync(uploadResult.FileKey);

        // Assert
        Assert.True(deleted);
        var exists = await _service.FileExistsAsync(uploadResult.FileKey);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteFileAsync_NonExistingFile_ReturnsFalse()
    {
        // Arrange
        var nonExistingKey = "non-existing/file.jpg";

        // Act
        var deleted = await _service.DeleteFileAsync(nonExistingKey);

        // Assert
        Assert.False(deleted);
    }

    [Fact]
    public async Task GetFileMetadataAsync_ExistingFile_ReturnsMetadata()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3, 4, 5 };
        using var stream = new MemoryStream(content);
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var uploadResult = await _service.UploadFileAsync(stream, fileName, contentType);

        // Act
        var metadata = await _service.GetFileMetadataAsync(uploadResult.FileKey);

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal(uploadResult.FileKey, metadata.FileKey);
        Assert.Equal(content.Length, metadata.SizeInBytes);
        Assert.Equal(contentType, metadata.ContentType);
    }

    [Fact]
    public async Task GetFileMetadataAsync_NonExistingFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistingKey = "non-existing/file.jpg";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => 
            _service.GetFileMetadataAsync(nonExistingKey));
    }

    [Fact]
    public async Task ListFilesAsync_WithNoFiles_ReturnsEmptyList()
    {
        // Act
        var files = await _service.ListFilesAsync();

        // Assert
        Assert.NotNull(files);
        Assert.Empty(files);
    }

    [Fact]
    public async Task ListFilesAsync_WithFiles_ReturnsFileList()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3, 4, 5 };
        using var stream1 = new MemoryStream(content);
        using var stream2 = new MemoryStream(content);
        
        var upload1 = await _service.UploadFileAsync(stream1, "test1.jpg", "image/jpeg");
        var upload2 = await _service.UploadFileAsync(stream2, "test2.jpg", "image/jpeg");

        // Act
        var files = await _service.ListFilesAsync();

        // Assert
        Assert.NotNull(files);
        Assert.Equal(2, files.Count());
        Assert.Contains(upload1.FileKey, files);
        Assert.Contains(upload2.FileKey, files);
    }

    [Fact]
    public async Task GetPresignedUrlAsync_ReturnsUrl()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3, 4, 5 };
        using var stream = new MemoryStream(content);
        var uploadResult = await _service.UploadFileAsync(stream, "test.jpg", "image/jpeg");

        // Act
        var url = await _service.GetPresignedUrlAsync(uploadResult.FileKey, TimeSpan.FromHours(1));

        // Assert
        Assert.NotNull(url);
        Assert.Contains(uploadResult.FileKey, url);
        Assert.Contains(_testBaseUrl, url);
    }

    [Fact]
    public void GetFileUrl_ReturnsCorrectUrl()
    {
        // Arrange
        var fileKey = "2024/01/01/test.jpg";

        // Act
        var url = _service.GetFileUrl(fileKey);

        // Assert
        Assert.Equal($"{_testBaseUrl}/{fileKey}", url);
    }

    [Fact]
    public async Task SaveFileAsync_LegacyMethod_StillWorks()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var fileName = "test.jpg";
        var folder = "slips";

        // Act
        var filePath = await _service.SaveFileAsync(content, fileName, folder);

        // Assert
        Assert.NotNull(filePath);
        Assert.Contains(folder, filePath);
        
        // Verify file exists
        var exists = await _service.FileExistsAsync(filePath);
        Assert.True(exists);
    }
}
