using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using SlipVerification.Application.DTOs.FileStorage;
using SlipVerification.Application.Interfaces;
using SlipVerification.Infrastructure.Configuration;

namespace SlipVerification.Infrastructure.Services.FileStorage;

/// <summary>
/// MinIO storage service implementation
/// </summary>
public class MinIOStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinIOConfiguration _config;
    private readonly ILogger<MinIOStorageService> _logger;

    public MinIOStorageService(
        IMinioClient minioClient,
        IOptions<MinIOConfiguration> config,
        ILogger<MinIOStorageService> logger)
    {
        _minioClient = minioClient;
        _config = config.Value;
        _logger = logger;
    }

    #region Legacy Methods (Backward Compatibility)

    public async Task<string> SaveFileAsync(byte[] fileData, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(fileData);
        var result = await UploadFileAsync(stream, fileName, "application/octet-stream", null, cancellationToken);
        return result.FileKey;
    }

    public Task<byte[]?> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return DownloadFileAsBytesAsync(filePath, cancellationToken);
    }

    public string GetFileUrl(string filePath)
    {
        return $"{(_config.UseSSL ? "https" : "http")}://{_config.Endpoint}/{_config.BucketName}/{filePath}";
    }

    #endregion

    public async Task<FileUploadResult> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        IDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure bucket exists
            await EnsureBucketExistsAsync(cancellationToken);

            // Generate unique file key
            var fileKey = GenerateFileKey(fileName);

            // Prepare metadata
            var fileMetadata = new Dictionary<string, string>(metadata ?? new Dictionary<string, string>())
            {
                ["original-filename"] = fileName,
                ["uploaded-at"] = DateTime.UtcNow.ToString("O")
            };

            // Upload file
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(fileKey)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType)
                .WithHeaders(fileMetadata);

            var response = await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            _logger.LogInformation(
                "File uploaded successfully to MinIO: {FileKey} ({Size} bytes)",
                fileKey,
                fileStream.Length
            );

            return new FileUploadResult
            {
                FileKey = fileKey,
                FileName = fileName,
                ContentType = contentType,
                SizeInBytes = fileStream.Length,
                Url = GetFileUrl(fileKey),
                ETag = response.Etag,
                UploadedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to MinIO: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var memoryStream = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(fileKey)
                .WithCallbackStream((stream) =>
                {
                    stream.CopyTo(memoryStream);
                });

            await _minioClient.GetObjectAsync(getObjectArgs, cancellationToken);

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from MinIO: {FileKey}", fileKey);
            throw;
        }
    }

    public async Task<string> GetPresignedUrlAsync(
        string fileKey,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(fileKey)
                .WithExpiry((int)expiration.TotalSeconds);

            return await _minioClient.PresignedGetObjectAsync(args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for MinIO: {FileKey}", fileKey);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(fileKey);

            await _minioClient.RemoveObjectAsync(args, cancellationToken);

            _logger.LogInformation("File deleted successfully from MinIO: {FileKey}", fileKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from MinIO: {FileKey}", fileKey);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(fileKey);

            await _minioClient.StatObjectAsync(args, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<FileMetadata> GetFileMetadataAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(fileKey);

            var objectStat = await _minioClient.StatObjectAsync(args, cancellationToken);

            return new FileMetadata
            {
                FileKey = fileKey,
                FileName = objectStat.MetaData.TryGetValue("original-filename", out var fileName) 
                    ? fileName 
                    : Path.GetFileName(fileKey),
                ContentType = objectStat.ContentType,
                SizeInBytes = objectStat.Size,
                CreatedAt = objectStat.LastModified,
                LastModified = objectStat.LastModified,
                CustomMetadata = objectStat.MetaData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file metadata from MinIO: {FileKey}", fileKey);
            throw;
        }
    }

    public async Task<IEnumerable<string>> ListFilesAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var files = new List<string>();

            var args = new ListObjectsArgs()
                .WithBucket(_config.BucketName)
                .WithPrefix(prefix)
                .WithRecursive(true);

            var observable = _minioClient.ListObjectsEnumAsync(args, cancellationToken);

            // Use async enumeration
            await foreach (var item in observable.WithCancellation(cancellationToken))
            {
                if (!item.IsDir)
                {
                    files.Add(item.Key);
                }
            }

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files from MinIO with prefix: {Prefix}", prefix);
            throw;
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var bucketExistsArgs = new BucketExistsArgs()
            .WithBucket(_config.BucketName);

        if (!await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken))
        {
            var makeBucketArgs = new MakeBucketArgs()
                .WithBucket(_config.BucketName);

            await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);

            _logger.LogInformation("Created MinIO bucket: {BucketName}", _config.BucketName);
        }
    }

    private string GenerateFileKey(string fileName)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var guid = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(fileName);

        return $"{timestamp}/{guid}{extension}";
    }

    private async Task<byte[]?> DownloadFileAsBytesAsync(string fileKey, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = await DownloadFileAsync(fileKey, cancellationToken);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }
        catch
        {
            return null;
        }
    }
}
