using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlipVerification.Application.DTOs.FileStorage;
using SlipVerification.Application.Interfaces;
using SlipVerification.Infrastructure.Configuration;

namespace SlipVerification.Infrastructure.Services.FileStorage;

/// <summary>
/// Azure Blob Storage service implementation
/// </summary>
public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureBlobConfiguration _config;
    private readonly ILogger<AzureBlobStorageService> _logger;
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(
        IOptions<AzureBlobConfiguration> config,
        ILogger<AzureBlobStorageService> logger)
    {
        _config = config.Value;
        _logger = logger;
        _blobServiceClient = new BlobServiceClient(_config.ConnectionString);
        _containerClient = _blobServiceClient.GetBlobContainerClient(_config.ContainerName);
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
        return GetBlobUrl(filePath);
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
            // Ensure container exists
            await EnsureContainerExistsAsync(cancellationToken);

            var fileKey = GenerateFileKey(fileName);
            var blobClient = _containerClient.GetBlobClient(fileKey);

            // Prepare metadata
            var blobMetadata = new Dictionary<string, string>(metadata ?? new Dictionary<string, string>())
            {
                ["originalfilename"] = fileName, // Azure doesn't allow hyphens in metadata keys
                ["uploadedat"] = DateTime.UtcNow.ToString("O")
            };

            // Upload options
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                },
                Metadata = blobMetadata
            };

            var response = await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken);

            _logger.LogInformation(
                "File uploaded successfully to Azure Blob: {FileKey} ({Size} bytes)",
                fileKey,
                fileStream.Length
            );

            return new FileUploadResult
            {
                FileKey = fileKey,
                FileName = fileName,
                ContentType = contentType,
                SizeInBytes = fileStream.Length,
                Url = GetBlobUrl(fileKey),
                ETag = response.Value.ETag.ToString(),
                UploadedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Azure Blob: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileKey);

            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from Azure Blob: {FileKey}", fileKey);
            throw;
        }
    }

    public Task<string> GetPresignedUrlAsync(
        string fileKey,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileKey);

            // Check if we can generate SAS tokens (requires shared key credentials)
            if (blobClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _config.ContainerName,
                    BlobName = fileKey,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiration)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                return Task.FromResult(sasUri.ToString());
            }
            else
            {
                // Fallback to regular URL if SAS token generation is not available
                _logger.LogWarning("Cannot generate SAS token for Azure Blob. Returning regular URL.");
                return Task.FromResult(blobClient.Uri.ToString());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for Azure Blob: {FileKey}", fileKey);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileKey);
            var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

            if (response.Value)
            {
                _logger.LogInformation("File deleted successfully from Azure Blob: {FileKey}", fileKey);
            }

            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from Azure Blob: {FileKey}", fileKey);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileKey);
            return await blobClient.ExistsAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    public async Task<FileMetadata> GetFileMetadataAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileKey);
            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

            return new FileMetadata
            {
                FileKey = fileKey,
                FileName = properties.Value.Metadata.TryGetValue("originalfilename", out var fileName) 
                    ? fileName 
                    : Path.GetFileName(fileKey),
                ContentType = properties.Value.ContentType,
                SizeInBytes = properties.Value.ContentLength,
                CreatedAt = properties.Value.CreatedOn.UtcDateTime,
                LastModified = properties.Value.LastModified.UtcDateTime,
                CustomMetadata = new Dictionary<string, string>(properties.Value.Metadata)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file metadata from Azure Blob: {FileKey}", fileKey);
            throw;
        }
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string? prefix = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var files = new List<string>();

            await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
            {
                files.Add(blobItem.Name);
            }

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files from Azure Blob with prefix: {Prefix}", prefix);
            throw;
        }
    }

    private async Task EnsureContainerExistsAsync(CancellationToken cancellationToken)
    {
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
    }

    private string GenerateFileKey(string fileName)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var guid = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(fileName);

        return $"{timestamp}/{guid}{extension}";
    }

    private string GetBlobUrl(string fileKey)
    {
        if (!string.IsNullOrEmpty(_config.CdnEndpoint))
        {
            return $"{_config.CdnEndpoint.TrimEnd('/')}/{fileKey}";
        }

        var blobClient = _containerClient.GetBlobClient(fileKey);
        return blobClient.Uri.ToString();
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
