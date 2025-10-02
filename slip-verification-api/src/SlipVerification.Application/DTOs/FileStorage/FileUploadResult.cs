namespace SlipVerification.Application.DTOs.FileStorage;

/// <summary>
/// Result of a file upload operation
/// </summary>
public class FileUploadResult
{
    /// <summary>
    /// Unique key/path of the uploaded file
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
    /// URL to access the file
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// ETag for cache validation
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Timestamp when file was uploaded
    /// </summary>
    public DateTime UploadedAt { get; set; }
}
