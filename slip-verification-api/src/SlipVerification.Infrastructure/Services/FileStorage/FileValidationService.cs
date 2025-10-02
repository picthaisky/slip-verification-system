using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlipVerification.Application.DTOs.FileStorage;
using SlipVerification.Application.Interfaces;
using SlipVerification.Infrastructure.Configuration;

namespace SlipVerification.Infrastructure.Services.FileStorage;

/// <summary>
/// Service for validating uploaded files
/// </summary>
public class FileValidationService : IFileValidationService
{
    private readonly FileValidationOptions _options;
    private readonly ILogger<FileValidationService> _logger;

    private readonly Dictionary<string, byte[]> _fileSignatures = new()
    {
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
        { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } }
    };

    public FileValidationService(
        IOptions<FileValidationOptions> options,
        ILogger<FileValidationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<FileValidationResult> ValidateFileAsync(
        Stream fileStream,
        string fileName,
        string contentType)
    {
        var result = new FileValidationResult { IsValid = true };

        // Check file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension))
        {
            result.IsValid = false;
            result.Errors.Add($"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", _options.AllowedExtensions)}");
        }

        // Check file size
        if (fileStream.Length > _options.MaxFileSizeInBytes)
        {
            result.IsValid = false;
            result.Errors.Add($"File size exceeds maximum allowed size of {_options.MaxFileSizeInBytes / 1024 / 1024}MB");
        }

        if (fileStream.Length == 0)
        {
            result.IsValid = false;
            result.Errors.Add("File is empty");
        }

        // Verify file signature (magic bytes)
        if (result.IsValid)
        {
            fileStream.Position = 0;
            var headerBytes = new byte[8];
            var bytesRead = await fileStream.ReadAsync(headerBytes, 0, headerBytes.Length);
            fileStream.Position = 0;

            if (bytesRead > 0 && _fileSignatures.TryGetValue(extension, out var signature))
            {
                if (!headerBytes.Take(signature.Length).SequenceEqual(signature))
                {
                    result.IsValid = false;
                    result.Errors.Add("File content does not match its extension");
                }
            }
        }

        // Virus scan (if enabled)
        if (result.IsValid && _options.EnableVirusScan)
        {
            var virusScanResult = await ScanForVirusesAsync(fileStream);
            if (!virusScanResult.IsClean)
            {
                result.IsValid = false;
                result.Errors.Add("File contains malicious content");
            }
        }

        if (!result.IsValid)
        {
            _logger.LogWarning("File validation failed for {FileName}: {Errors}", fileName, string.Join("; ", result.Errors));
        }

        return result;
    }

    private async Task<VirusScanResult> ScanForVirusesAsync(Stream fileStream)
    {
        // Integrate with ClamAV or other antivirus service
        // This is a placeholder for future implementation
        await Task.CompletedTask;
        return new VirusScanResult { IsClean = true };
    }

    private class VirusScanResult
    {
        public bool IsClean { get; set; }
    }
}
