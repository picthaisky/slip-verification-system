using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SlipVerification.Application.DTOs.FileStorage;
using SlipVerification.Application.Interfaces;

namespace SlipVerification.API.Controllers.v1;

/// <summary>
/// File storage endpoints for managing uploaded files
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _storageService;
    private readonly IFileValidationService _validationService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileStorageService storageService,
        IFileValidationService validationService,
        ILogger<FilesController> logger)
    {
        _storageService = storageService;
        _validationService = validationService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file to storage
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload result with file information</returns>
    [HttpPost("upload")]
    [RequestSizeLimit(10_485_760)] // 10MB
    [ProducesResponseType(typeof(FileUploadResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadFile(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        try
        {
            // Validate file
            await using var fileStream = file.OpenReadStream();
            var validationResult = await _validationService.ValidateFileAsync(
                fileStream,
                file.FileName,
                file.ContentType
            );

            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors });
            }

            // Upload file
            fileStream.Position = 0;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var metadata = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(userId))
            {
                metadata["uploaded-by"] = userId;
            }

            var uploadResult = await _storageService.UploadFileAsync(
                fileStream,
                file.FileName,
                file.ContentType,
                metadata,
                cancellationToken
            );

            _logger.LogInformation(
                "File uploaded successfully: {FileKey} by user {UserId}",
                uploadResult.FileKey,
                userId
            );

            return Ok(uploadResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
            return StatusCode(500, new { error = "An error occurred while uploading the file" });
        }
    }

    /// <summary>
    /// Download a file from storage
    /// </summary>
    /// <param name="fileKey">Unique file key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File stream</returns>
    [HttpGet("{fileKey}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DownloadFile(
        string fileKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _storageService.FileExistsAsync(fileKey, cancellationToken);
            if (!exists)
            {
                return NotFound(new { error = "File not found" });
            }

            var fileStream = await _storageService.DownloadFileAsync(fileKey, cancellationToken);
            var metadata = await _storageService.GetFileMetadataAsync(fileKey, cancellationToken);

            return File(fileStream, metadata.ContentType, metadata.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "File not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FileKey}", fileKey);
            return StatusCode(500, new { error = "An error occurred while downloading the file" });
        }
    }

    /// <summary>
    /// Get a presigned URL for temporary file access
    /// </summary>
    /// <param name="fileKey">Unique file key</param>
    /// <param name="expirationMinutes">URL expiration time in minutes (default: 60)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Presigned URL</returns>
    [HttpGet("{fileKey}/url")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPresignedUrl(
        string fileKey,
        [FromQuery] int expirationMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _storageService.FileExistsAsync(fileKey, cancellationToken);
            if (!exists)
            {
                return NotFound(new { error = "File not found" });
            }

            var url = await _storageService.GetPresignedUrlAsync(
                fileKey,
                TimeSpan.FromMinutes(expirationMinutes)
            );

            return Ok(new 
            { 
                url, 
                expiresIn = expirationMinutes * 60,
                expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL: {FileKey}", fileKey);
            return StatusCode(500, new { error = "An error occurred while generating the URL" });
        }
    }

    /// <summary>
    /// Get file metadata
    /// </summary>
    /// <param name="fileKey">Unique file key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File metadata</returns>
    [HttpGet("{fileKey}/metadata")]
    [ProducesResponseType(typeof(FileMetadata), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFileMetadata(
        string fileKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var metadata = await _storageService.GetFileMetadataAsync(fileKey, cancellationToken);
            return Ok(metadata);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "File not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file metadata: {FileKey}", fileKey);
            return StatusCode(500, new { error = "An error occurred while retrieving metadata" });
        }
    }

    /// <summary>
    /// List files with optional prefix filter
    /// </summary>
    /// <param name="prefix">Optional prefix to filter files</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of file keys</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListFiles(
        [FromQuery] string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var files = await _storageService.ListFilesAsync(prefix, cancellationToken);
            return Ok(new { files, count = files.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files with prefix: {Prefix}", prefix);
            return StatusCode(500, new { error = "An error occurred while listing files" });
        }
    }

    /// <summary>
    /// Delete a file from storage
    /// </summary>
    /// <param name="fileKey">Unique file key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{fileKey}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteFile(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _storageService.DeleteFileAsync(fileKey, cancellationToken);

            if (deleted)
            {
                _logger.LogInformation("File deleted: {FileKey}", fileKey);
                return NoContent();
            }

            return NotFound(new { error = "File not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileKey}", fileKey);
            return StatusCode(500, new { error = "An error occurred while deleting the file" });
        }
    }

    /// <summary>
    /// Check if a file exists
    /// </summary>
    /// <param name="fileKey">Unique file key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Existence status</returns>
    [HttpHead("{fileKey}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FileExists(
        string fileKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _storageService.FileExistsAsync(fileKey, cancellationToken);
            return exists ? Ok() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FileKey}", fileKey);
            return StatusCode(500);
        }
    }
}
