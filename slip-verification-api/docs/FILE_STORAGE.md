# File Storage System

This document describes the file storage system implementation with support for multiple storage providers.

## Supported Storage Providers

1. **Local File Storage** - For development and testing
2. **MinIO** - Self-hosted S3-compatible object storage
3. **AWS S3** - Cloud-based object storage
4. **Azure Blob Storage** - Microsoft Azure cloud storage

## Configuration

### Local Storage (Default)

```json
{
  "FileStorage": {
    "Provider": "Local",
    "BasePath": "uploads",
    "BaseUrl": "http://localhost:5000/uploads"
  }
}
```

### MinIO Storage

```json
{
  "FileStorage": {
    "Provider": "MinIO"
  },
  "MinIO": {
    "Endpoint": "localhost:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "BucketName": "slip-files",
    "UseSSL": false,
    "Region": "us-east-1"
  }
}
```

### AWS S3 Storage

```json
{
  "FileStorage": {
    "Provider": "S3"
  },
  "S3": {
    "Region": "us-east-1",
    "BucketName": "slip-files",
    "AccessKeyId": "YOUR_ACCESS_KEY",
    "SecretAccessKey": "YOUR_SECRET_KEY",
    "CloudFrontDistributionDomain": "d1234567890.cloudfront.net"
  }
}
```

### Azure Blob Storage

```json
{
  "FileStorage": {
    "Provider": "AzureBlob"
  },
  "AzureBlob": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...",
    "ContainerName": "slip-files",
    "CdnEndpoint": "https://your-cdn.azureedge.net"
  }
}
```

### File Validation Options

```json
{
  "FileValidation": {
    "MaxFileSizeInBytes": 10485760,
    "EnableVirusScan": false,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".pdf"]
  }
}
```

## API Endpoints

### Upload File
```
POST /api/v1/files/upload
Content-Type: multipart/form-data

Form field: file (IFormFile)
```

### Download File
```
GET /api/v1/files/{fileKey}/download
```

### Get Presigned URL
```
GET /api/v1/files/{fileKey}/url?expirationMinutes=60
```

### Get File Metadata
```
GET /api/v1/files/{fileKey}/metadata
```

### List Files (Admin only)
```
GET /api/v1/files?prefix=2024/01/
```

### Delete File (Admin only)
```
DELETE /api/v1/files/{fileKey}
```

### Check File Existence
```
HEAD /api/v1/files/{fileKey}
```

## Features

### File Validation
- **File Extension Validation**: Only allows .jpg, .jpeg, .png, .pdf
- **Magic Bytes Validation**: Verifies file content matches extension
- **File Size Limits**: Default 10MB maximum
- **Virus Scanning**: Optional integration (placeholder for ClamAV)

### Security
- **Presigned URLs**: Time-limited secure access
- **Access Control**: Role-based authorization (Admin role required for delete/list)
- **Encryption at Rest**: 
  - S3: AES-256 server-side encryption
  - Azure: Encryption enabled by default
  - MinIO: Configurable
- **Input Sanitization**: All file uploads are validated

### Performance
- **Intelligent Tiering**: AWS S3 uses intelligent tiering
- **CDN Support**: CloudFront (AWS) and Azure CDN integration
- **Async Operations**: All file operations are asynchronous
- **Stream-based Processing**: Efficient memory usage

## Usage Example

```csharp
// Inject IFileStorageService and IFileValidationService
public class MyService
{
    private readonly IFileStorageService _storage;
    private readonly IFileValidationService _validation;

    public MyService(IFileStorageService storage, IFileValidationService validation)
    {
        _storage = storage;
        _validation = validation;
    }

    public async Task<FileUploadResult> UploadAsync(Stream fileStream, string fileName)
    {
        // Validate file
        var validation = await _validation.ValidateFileAsync(fileStream, fileName, "image/jpeg");
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(string.Join(", ", validation.Errors));
        }

        // Upload file
        fileStream.Position = 0;
        var result = await _storage.UploadFileAsync(
            fileStream, 
            fileName, 
            "image/jpeg",
            new Dictionary<string, string> { ["uploaded-by"] = "user123" }
        );

        return result;
    }
}
```

## Migration from Legacy Interface

The enhanced interface maintains backward compatibility:

```csharp
// Legacy methods still work
var filePath = await _storage.SaveFileAsync(fileBytes, fileName, folder);
var fileBytes = await _storage.GetFileAsync(filePath);
var deleted = await _storage.DeleteFileAsync(filePath);
var url = _storage.GetFileUrl(filePath);

// New methods provide enhanced functionality
var uploadResult = await _storage.UploadFileAsync(stream, fileName, contentType, metadata);
var downloadStream = await _storage.DownloadFileAsync(fileKey);
var presignedUrl = await _storage.GetPresignedUrlAsync(fileKey, TimeSpan.FromHours(1));
var exists = await _storage.FileExistsAsync(fileKey);
var metadata = await _storage.GetFileMetadataAsync(fileKey);
var files = await _storage.ListFilesAsync(prefix);
```

## Docker Compose - MinIO Setup

```yaml
services:
  minio:
    image: minio/minio:latest
    ports:
      - "9000:9000"
      - "9001:9001"
    environment:
      MINIO_ROOT_USER: minioadmin
      MINIO_ROOT_PASSWORD: minioadmin
    command: server /data --console-address ":9001"
    volumes:
      - minio-data:/data

volumes:
  minio-data:
```

## Testing

The implementation includes comprehensive unit tests:
- FileValidationService: 10 tests
- LocalFileStorageService: 17 tests

Run tests:
```bash
cd slip-verification-api
dotnet test tests/SlipVerification.UnitTests --filter "FullyQualifiedName~FileStorage"
```

## Production Considerations

1. **Storage Provider Selection**:
   - Use Local storage for development only
   - Use MinIO for self-hosted deployments
   - Use S3 for AWS-based deployments
   - Use Azure Blob for Azure-based deployments

2. **Security**:
   - Enable virus scanning in production
   - Use HTTPS/SSL for all storage connections
   - Rotate access keys regularly
   - Implement proper IAM policies

3. **Performance**:
   - Enable CDN for public file access
   - Use presigned URLs for temporary access
   - Consider multipart uploads for large files (future enhancement)

4. **Backup**:
   - Enable versioning on S3 buckets
   - Set up lifecycle policies for old files
   - Regular backup of MinIO data

5. **Monitoring**:
   - Monitor storage usage and costs
   - Set up alerts for failed uploads
   - Track presigned URL usage
