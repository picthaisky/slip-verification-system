namespace SlipVerification.Application.DTOs.FileStorage;

/// <summary>
/// Metadata information about a stored file
/// </summary>
public class FileMetadata
{
    /// <summary>
    /// Unique key/path of the file
    /// </summary>
    public string FileKey { get; set; } = string.Empty;

    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Content type of the file
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long SizeInBytes { get; set; }

    /// <summary>
    /// Timestamp when file was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when file was last modified
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Custom metadata key-value pairs
    /// </summary>
    public IDictionary<string, string> CustomMetadata { get; set; } = new Dictionary<string, string>();
}
