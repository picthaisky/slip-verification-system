using SlipVerification.Application.DTOs.FileStorage;
using SlipVerification.Application.Interfaces;

namespace SlipVerification.Infrastructure.Services;

/// <summary>
/// Local file storage service implementation
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    public LocalFileStorageService(string basePath, string baseUrl)
    {
        _basePath = basePath;
        _baseUrl = baseUrl;
        
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> SaveFileAsync(byte[] fileData, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var folderPath = Path.Combine(_basePath, folder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        await File.WriteAllBytesAsync(filePath, fileData, cancellationToken);

        return Path.Combine(folder, uniqueFileName).Replace("\\", "/");
    }

    public Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public async Task<byte[]?> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    public string GetFileUrl(string filePath)
    {
        return $"{_baseUrl.TrimEnd('/')}/{filePath}";
    }

    public async Task<FileUploadResult> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        IDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var fileKey = GenerateFileKey(fileName);
        var folderPath = Path.Combine(_basePath, Path.GetDirectoryName(fileKey) ?? string.Empty);
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fullPath = Path.Combine(_basePath, fileKey);
        
        using var fileOutputStream = File.Create(fullPath);
        await fileStream.CopyToAsync(fileOutputStream, cancellationToken);

        var fileInfo = new FileInfo(fullPath);

        return new FileUploadResult
        {
            FileKey = fileKey,
            FileName = fileName,
            ContentType = contentType,
            SizeInBytes = fileInfo.Length,
            Url = GetFileUrl(fileKey),
            ETag = Convert.ToBase64String(BitConverter.GetBytes(fileInfo.LastWriteTimeUtc.Ticks)),
            UploadedAt = DateTime.UtcNow
        };
    }

    public async Task<Stream> DownloadFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, fileKey);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {fileKey}");
        }

        var memoryStream = new MemoryStream();
        using var fileStream = File.OpenRead(fullPath);
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public Task<string> GetPresignedUrlAsync(
        string fileKey,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        // For local storage, just return the regular URL
        // In a production scenario, you might implement token-based temporary URLs
        return Task.FromResult(GetFileUrl(fileKey));
    }

    public Task<bool> FileExistsAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, fileKey);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<FileMetadata> GetFileMetadataAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, fileKey);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {fileKey}");
        }

        var fileInfo = new FileInfo(fullPath);
        var fileName = Path.GetFileName(fileKey);

        return Task.FromResult(new FileMetadata
        {
            FileKey = fileKey,
            FileName = fileName,
            ContentType = GetContentType(fileName),
            SizeInBytes = fileInfo.Length,
            CreatedAt = fileInfo.CreationTimeUtc,
            LastModified = fileInfo.LastWriteTimeUtc,
            CustomMetadata = new Dictionary<string, string>()
        });
    }

    public Task<IEnumerable<string>> ListFilesAsync(string? prefix = null, CancellationToken cancellationToken = default)
    {
        var searchPath = string.IsNullOrEmpty(prefix) 
            ? _basePath 
            : Path.Combine(_basePath, prefix);

        if (!Directory.Exists(searchPath))
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        var files = Directory.GetFiles(searchPath, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(_basePath, f).Replace("\\", "/"));

        return Task.FromResult(files);
    }

    private string GenerateFileKey(string fileName)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var guid = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(fileName);
        
        return $"{timestamp}/{guid}{extension}";
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
