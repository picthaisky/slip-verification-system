using SlipVerification.Application.DTOs.FileStorage;

namespace SlipVerification.Application.Interfaces;

/// <summary>
/// Interface for file storage service
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to storage (legacy method)
    /// </summary>
    Task<string> SaveFileAsync(byte[] fileData, string fileName, string folder, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a file from storage (legacy method)
    /// </summary>
    Task<byte[]?> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the URL of a file
    /// </summary>
    string GetFileUrl(string filePath);

    /// <summary>
    /// Uploads a file with enhanced metadata support
    /// </summary>
    Task<FileUploadResult> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        IDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Downloads a file as a stream
    /// </summary>
    Task<Stream> DownloadFileAsync(
        string fileKey,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generates a presigned URL for secure temporary access
    /// </summary>
    Task<string> GetPresignedUrlAsync(
        string fileKey,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a file exists in storage
    /// </summary>
    Task<bool> FileExistsAsync(
        string fileKey,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets metadata for a file
    /// </summary>
    Task<FileMetadata> GetFileMetadataAsync(
        string fileKey,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lists files in storage with optional prefix filter
    /// </summary>
    Task<IEnumerable<string>> ListFilesAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default);
}
