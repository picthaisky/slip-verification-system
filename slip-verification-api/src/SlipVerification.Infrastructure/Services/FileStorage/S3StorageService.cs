using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlipVerification.Application.DTOs.FileStorage;
using SlipVerification.Application.Interfaces;
using SlipVerification.Infrastructure.Configuration;

namespace SlipVerification.Infrastructure.Services.FileStorage;

/// <summary>
/// AWS S3 storage service implementation
/// </summary>
public class S3StorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Configuration _config;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(
        IAmazonS3 s3Client,
        IOptions<S3Configuration> config,
        ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
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
        return GetCloudFrontUrl(filePath);
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
            var fileKey = GenerateFileKey(fileName);

            var request = new PutObjectRequest
            {
                BucketName = _config.BucketName,
                Key = fileKey,
                InputStream = fileStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.Private,
                StorageClass = S3StorageClass.IntelligentTiering,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            // Add metadata
            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    request.Metadata.Add(kvp.Key, kvp.Value);
                }
            }

            request.Metadata.Add("original-filename", fileName);
            request.Metadata.Add("uploaded-at", DateTime.UtcNow.ToString("O"));

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);

            _logger.LogInformation(
                "File uploaded successfully to S3: {FileKey} ({Size} bytes)",
                fileKey,
                fileStream.Length
            );

            return new FileUploadResult
            {
                FileKey = fileKey,
                FileName = fileName,
                ContentType = contentType,
                SizeInBytes = fileStream.Length,
                Url = GetCloudFrontUrl(fileKey),
                ETag = response.ETag,
                UploadedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _config.BucketName,
                Key = fileKey
            };

            var response = await _s3Client.GetObjectAsync(request, cancellationToken);

            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from S3: {FileKey}", fileKey);
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
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _config.BucketName,
                Key = fileKey,
                Expires = DateTime.UtcNow.Add(expiration),
                Verb = HttpVerb.GET
            };

            var url = _s3Client.GetPreSignedURL(request);
            return Task.FromResult(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for S3: {FileKey}", fileKey);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _config.BucketName,
                Key = fileKey
            };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);

            _logger.LogInformation("File deleted successfully from S3: {FileKey}", fileKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from S3: {FileKey}", fileKey);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _config.BucketName,
                Key = fileKey
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<FileMetadata> GetFileMetadataAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _config.BucketName,
                Key = fileKey
            };

            var response = await _s3Client.GetObjectMetadataAsync(request, cancellationToken);

            var customMetadata = new Dictionary<string, string>();
            foreach (var key in response.Metadata.Keys)
            {
                customMetadata[key] = response.Metadata[key];
            }

            var fileName = Path.GetFileName(fileKey);
            if (response.Metadata.Keys.Contains("x-amz-meta-original-filename"))
            {
                fileName = response.Metadata["x-amz-meta-original-filename"];
            }

            return new FileMetadata
            {
                FileKey = fileKey,
                FileName = fileName,
                ContentType = response.Headers.ContentType,
                SizeInBytes = response.ContentLength,
                CreatedAt = response.LastModified,
                LastModified = response.LastModified,
                CustomMetadata = customMetadata
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file metadata from S3: {FileKey}", fileKey);
            throw;
        }
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string? prefix = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var files = new List<string>();
            string? continuationToken = null;

            do
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _config.BucketName,
                    Prefix = prefix,
                    ContinuationToken = continuationToken
                };

                var response = await _s3Client.ListObjectsV2Async(request, cancellationToken);

                files.AddRange(response.S3Objects.Select(obj => obj.Key));

                continuationToken = response.IsTruncated ? response.NextContinuationToken : null;

            } while (continuationToken != null);

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files from S3 with prefix: {Prefix}", prefix);
            throw;
        }
    }

    private string GenerateFileKey(string fileName)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var guid = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(fileName);

        return $"{timestamp}/{guid}{extension}";
    }

    private string GetCloudFrontUrl(string fileKey)
    {
        if (!string.IsNullOrEmpty(_config.CloudFrontDistributionDomain))
        {
            return $"https://{_config.CloudFrontDistributionDomain}/{fileKey}";
        }

        return $"https://{_config.BucketName}.s3.{_config.Region}.amazonaws.com/{fileKey}";
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
