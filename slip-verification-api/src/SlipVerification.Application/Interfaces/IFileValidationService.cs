using SlipVerification.Application.DTOs.FileStorage;

namespace SlipVerification.Application.Interfaces;

/// <summary>
/// Interface for file validation service
/// </summary>
public interface IFileValidationService
{
    /// <summary>
    /// Validates a file for security and compliance
    /// </summary>
    /// <param name="fileStream">The file stream to validate</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME content type</param>
    /// <returns>Validation result with errors if any</returns>
    Task<FileValidationResult> ValidateFileAsync(
        Stream fileStream,
        string fileName,
        string contentType);
}
