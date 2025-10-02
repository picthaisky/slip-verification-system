namespace SlipVerification.Infrastructure.Configuration;

/// <summary>
/// Configuration for file validation
/// </summary>
public class FileValidationOptions
{
    /// <summary>
    /// Maximum file size in bytes (default: 10MB)
    /// </summary>
    public long MaxFileSizeInBytes { get; set; } = 10_485_760; // 10MB

    /// <summary>
    /// Enable virus scanning (default: false)
    /// </summary>
    public bool EnableVirusScan { get; set; } = false;

    /// <summary>
    /// Allowed file extensions
    /// </summary>
    public HashSet<string> AllowedExtensions { get; set; } = new() { ".jpg", ".jpeg", ".png", ".pdf" };
}
