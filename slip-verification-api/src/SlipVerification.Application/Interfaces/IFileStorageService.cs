namespace SlipVerification.Application.Interfaces;

/// <summary>
/// Interface for file storage service
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to storage
    /// </summary>
    Task<string> SaveFileAsync(byte[] fileData, string fileName, string folder, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a file from storage
    /// </summary>
    Task<byte[]?> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the URL of a file
    /// </summary>
    string GetFileUrl(string filePath);
}
