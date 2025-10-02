namespace SlipVerification.Infrastructure.Configuration;

/// <summary>
/// Configuration for Azure Blob Storage
/// </summary>
public class AzureBlobConfiguration
{
    /// <summary>
    /// Azure Storage connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Container name for storing files
    /// </summary>
    public string ContainerName { get; set; } = "slip-files";

    /// <summary>
    /// Azure CDN endpoint (optional)
    /// </summary>
    public string? CdnEndpoint { get; set; }
}
