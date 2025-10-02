namespace SlipVerification.Infrastructure.Configuration;

/// <summary>
/// Configuration for MinIO storage
/// </summary>
public class MinIOConfiguration
{
    /// <summary>
    /// MinIO server endpoint (e.g., "localhost:9000")
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Access key for authentication
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Secret key for authentication
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Bucket name for storing files
    /// </summary>
    public string BucketName { get; set; } = "slip-files";

    /// <summary>
    /// Use SSL/TLS for connections
    /// </summary>
    public bool UseSSL { get; set; } = false;

    /// <summary>
    /// Region name (optional)
    /// </summary>
    public string? Region { get; set; }
}
