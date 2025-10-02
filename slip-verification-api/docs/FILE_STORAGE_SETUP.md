# File Storage Service - Dependency Injection Setup Guide

This guide shows how to configure the file storage services in your `Program.cs`.

## Step 1: Add Configuration Options

Add this code before registering the file storage service:

```csharp
using SlipVerification.Infrastructure.Configuration;
using SlipVerification.Infrastructure.Services.FileStorage;
using Minio;
using Amazon.S3;
using Amazon;

// Configure file validation options
builder.Services.Configure<FileValidationOptions>(
    builder.Configuration.GetSection("FileValidation"));

// Configure storage provider options based on selected provider
var storageProvider = builder.Configuration["FileStorage:Provider"] ?? "Local";

if (storageProvider == "MinIO")
{
    builder.Services.Configure<MinIOConfiguration>(
        builder.Configuration.GetSection("MinIO"));
}
else if (storageProvider == "S3")
{
    builder.Services.Configure<S3Configuration>(
        builder.Configuration.GetSection("S3"));
}
else if (storageProvider == "AzureBlob")
{
    builder.Services.Configure<AzureBlobConfiguration>(
        builder.Configuration.GetSection("AzureBlob"));
}
```

## Step 2: Register File Validation Service

```csharp
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
```

## Step 3: Register File Storage Service Based on Provider

Replace the existing `IFileStorageService` registration (around line 74) with:

```csharp
// Register file storage service based on configuration
var storageProvider = builder.Configuration["FileStorage:Provider"] ?? "Local";

switch (storageProvider)
{
    case "Local":
        builder.Services.AddSingleton<IFileStorageService>(sp =>
        {
            var basePath = builder.Configuration["FileStorage:BasePath"] ?? "uploads";
            var baseUrl = builder.Configuration["FileStorage:BaseUrl"] ?? "http://localhost:5000/uploads";
            var logger = sp.GetRequiredService<ILogger<LocalFileStorageService>>();
            return new LocalFileStorageService(basePath, baseUrl);
        });
        break;

    case "MinIO":
        builder.Services.AddSingleton<IMinioClient>(sp =>
        {
            var config = builder.Configuration.GetSection("MinIO").Get<MinIOConfiguration>()
                ?? throw new InvalidOperationException("MinIO configuration not found");
            
            return new MinioClient()
                .WithEndpoint(config.Endpoint)
                .WithCredentials(config.AccessKey, config.SecretKey)
                .WithSSL(config.UseSSL)
                .Build();
        });
        builder.Services.AddScoped<IFileStorageService, MinIOStorageService>();
        break;

    case "S3":
        builder.Services.AddSingleton<IAmazonS3>(sp =>
        {
            var config = builder.Configuration.GetSection("S3").Get<S3Configuration>()
                ?? throw new InvalidOperationException("S3 configuration not found");
            
            var awsConfig = new Amazon.S3.AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(config.Region)
            };
            
            // Use credentials from configuration if provided, otherwise use default credential chain
            if (!string.IsNullOrEmpty(config.AccessKeyId) && !string.IsNullOrEmpty(config.SecretAccessKey))
            {
                return new AmazonS3Client(config.AccessKeyId, config.SecretAccessKey, awsConfig);
            }
            else
            {
                // Use default AWS credential chain (IAM role, environment variables, etc.)
                return new AmazonS3Client(awsConfig);
            }
        });
        builder.Services.AddScoped<IFileStorageService, S3StorageService>();
        break;

    case "AzureBlob":
        builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        break;

    default:
        throw new InvalidOperationException($"Unknown storage provider: {storageProvider}");
}
```

## Complete Example

Here's a complete example of what to add to `Program.cs`:

```csharp
// Add this at the top of the file
using SlipVerification.Infrastructure.Configuration;
using SlipVerification.Infrastructure.Services.FileStorage;
using Minio;
using Amazon.S3;
using Amazon;

// ... existing code ...

// Around line 70-80, replace the file storage registration with:

// Configure file validation options
builder.Services.Configure<FileValidationOptions>(
    builder.Configuration.GetSection("FileValidation"));

// Register file validation service
builder.Services.AddScoped<IFileValidationService, FileValidationService>();

// Register file storage service based on configuration
var storageProvider = builder.Configuration["FileStorage:Provider"] ?? "Local";

switch (storageProvider)
{
    case "Local":
        builder.Services.AddSingleton<IFileStorageService>(sp =>
        {
            var basePath = builder.Configuration["FileStorage:BasePath"] ?? "uploads";
            var baseUrl = builder.Configuration["FileStorage:BaseUrl"] ?? "http://localhost:5000/uploads";
            return new LocalFileStorageService(basePath, baseUrl);
        });
        Log.Information("Using Local File Storage at {BasePath}", builder.Configuration["FileStorage:BasePath"]);
        break;

    case "MinIO":
        builder.Services.Configure<MinIOConfiguration>(builder.Configuration.GetSection("MinIO"));
        builder.Services.AddSingleton<IMinioClient>(sp =>
        {
            var config = builder.Configuration.GetSection("MinIO").Get<MinIOConfiguration>()
                ?? throw new InvalidOperationException("MinIO configuration not found");
            
            return new MinioClient()
                .WithEndpoint(config.Endpoint)
                .WithCredentials(config.AccessKey, config.SecretKey)
                .WithSSL(config.UseSSL)
                .Build();
        });
        builder.Services.AddScoped<IFileStorageService, MinIOStorageService>();
        Log.Information("Using MinIO File Storage at {Endpoint}", builder.Configuration["MinIO:Endpoint"]);
        break;

    case "S3":
        builder.Services.Configure<S3Configuration>(builder.Configuration.GetSection("S3"));
        builder.Services.AddSingleton<IAmazonS3>(sp =>
        {
            var config = builder.Configuration.GetSection("S3").Get<S3Configuration>()
                ?? throw new InvalidOperationException("S3 configuration not found");
            
            var awsConfig = new Amazon.S3.AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(config.Region)
            };
            
            if (!string.IsNullOrEmpty(config.AccessKeyId) && !string.IsNullOrEmpty(config.SecretAccessKey))
            {
                return new AmazonS3Client(config.AccessKeyId, config.SecretAccessKey, awsConfig);
            }
            else
            {
                return new AmazonS3Client(awsConfig);
            }
        });
        builder.Services.AddScoped<IFileStorageService, S3StorageService>();
        Log.Information("Using AWS S3 File Storage in region {Region}", builder.Configuration["S3:Region"]);
        break;

    case "AzureBlob":
        builder.Services.Configure<AzureBlobConfiguration>(builder.Configuration.GetSection("AzureBlob"));
        builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        Log.Information("Using Azure Blob Storage");
        break;

    default:
        throw new InvalidOperationException($"Unknown storage provider: {storageProvider}");
}
```

## Quick Switch Between Providers

To switch between providers, simply change the `Provider` value in `appsettings.json`:

```json
{
  "FileStorage": {
    "Provider": "MinIO"  // Change to: "Local", "S3", or "AzureBlob"
  }
}
```

## Environment-Specific Configuration

Use `appsettings.Development.json` for local development:

```json
{
  "FileStorage": {
    "Provider": "Local"
  }
}
```

Use `appsettings.Production.json` for production:

```json
{
  "FileStorage": {
    "Provider": "S3"
  },
  "S3": {
    "Region": "us-east-1",
    "BucketName": "production-slip-files"
  }
}
```

## Testing the Configuration

After setup, you can test the file storage service:

```bash
# Test file upload
curl -X POST "http://localhost:5000/api/v1/files/upload" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@test.jpg"

# Test file download
curl -X GET "http://localhost:5000/api/v1/files/{fileKey}/download" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -o downloaded.jpg

# Test presigned URL
curl -X GET "http://localhost:5000/api/v1/files/{fileKey}/url?expirationMinutes=60" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Troubleshooting

### MinIO Connection Issues
- Ensure MinIO is running: `docker-compose up minio`
- Check endpoint configuration matches MinIO server
- Verify access credentials are correct

### S3 Permission Issues
- Ensure IAM role has `s3:PutObject`, `s3:GetObject`, `s3:DeleteObject` permissions
- Check bucket policy allows access
- Verify region configuration

### Azure Blob Issues
- Validate connection string format
- Ensure container exists or service has permission to create it
- Check firewall rules if using Azure Storage firewall
