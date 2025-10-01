using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlipVerification.Application.DTOs.Notifications;
using SlipVerification.Application.Interfaces.Notifications;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Infrastructure.Services.Notifications;

/// <summary>
/// LINE Notify channel implementation
/// </summary>
public class LineNotifyChannel : INotificationChannel
{
    private readonly ILogger<LineNotifyChannel> _logger;
    private readonly LineNotifyOptions _options;
    private readonly HttpClient _httpClient;
    private const string LineNotifyApiUrl = "https://notify-api.line.me/api/notify";

    public NotificationChannel ChannelType => NotificationChannel.LINE;

    public LineNotifyChannel(
        ILogger<LineNotifyChannel> logger,
        IOptions<LineNotifyOptions> options,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClientFactory.CreateClient("LineNotify");
    }

    public bool SupportsChannel(NotificationChannel channel)
    {
        return channel == NotificationChannel.LINE;
    }

    public async Task<NotificationResult> SendAsync(NotificationMessage message)
    {
        try
        {
            if (string.IsNullOrEmpty(message.LineToken))
            {
                return NotificationResult.CreateFailure("LINE token is required");
            }

            var content = new MultipartFormDataContent();
            content.Add(new StringContent($"{message.Title}\n{message.Message}"), "message");

            // Add image if provided
            if (!string.IsNullOrEmpty(message.ImagePath))
            {
                content.Add(new StringContent(message.ImagePath), "imageThumbnail");
                content.Add(new StringContent(message.ImagePath), "imageFullsize");
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, LineNotifyApiUrl)
            {
                Content = content
            };
            request.Headers.Add("Authorization", $"Bearer {message.LineToken}");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("LINE notification sent successfully to user {UserId}", message.UserId);
                return NotificationResult.CreateSuccess(null, response.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault());
            }

            _logger.LogError("Failed to send LINE notification. Status: {StatusCode}, Response: {Response}",
                response.StatusCode, responseContent);
            return NotificationResult.CreateFailure($"LINE API error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending LINE notification to user {UserId}", message.UserId);
            return NotificationResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Configuration options for LINE Notify
/// </summary>
public class LineNotifyOptions
{
    public const string SectionName = "LineNotify";
    
    /// <summary>
    /// Default LINE token (optional, can be overridden per message)
    /// </summary>
    public string? DefaultToken { get; set; }
    
    /// <summary>
    /// API timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
