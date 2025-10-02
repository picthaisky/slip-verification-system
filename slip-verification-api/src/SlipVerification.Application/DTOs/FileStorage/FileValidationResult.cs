namespace SlipVerification.Application.DTOs.FileStorage;

/// <summary>
/// Result of file validation
/// </summary>
public class FileValidationResult
{
    /// <summary>
    /// Indicates if the file passed validation
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
