using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlipVerification.Application.DTOs.Notifications;
using SlipVerification.Application.Interfaces.Notifications;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Infrastructure.Services.Notifications;

/// <summary>
/// Push notification channel implementation using Firebase Cloud Messaging (FCM)
/// </summary>
public class PushNotificationChannel : INotificationChannel
{
    private readonly ILogger<PushNotificationChannel> _logger;
    private readonly PushNotificationOptions _options;
    private readonly HttpClient _httpClient;
    private const string FcmApiUrl = "https://fcm.googleapis.com/v1/projects/{0}/messages:send";

    public NotificationChannel ChannelType => NotificationChannel.PUSH;

    public PushNotificationChannel(
        ILogger<PushNotificationChannel> logger,
        IOptions<PushNotificationOptions> options,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClientFactory.CreateClient("FCM");
    }

    public bool SupportsChannel(NotificationChannel channel)
    {
        return channel == NotificationChannel.PUSH;
    }

    public async Task<NotificationResult> SendAsync(NotificationMessage message)
    {
        try
        {
            if (string.IsNullOrEmpty(message.DeviceToken))
            {
                return NotificationResult.CreateFailure("Device token is required");
            }

            var payload = new
            {
                message = new
                {
                    token = message.DeviceToken,
                    notification = new
                    {
                        title = message.Title,
                        body = message.Message
                    },
                    data = message.Data ?? new Dictionary<string, object>(),
                    android = new
                    {
                        priority = GetAndroidPriority(message.Priority)
                    },
                    apns = new
                    {
                        headers = new
                        {
                            apns_priority = GetApnsPriority(message.Priority)
                        }
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = string.Format(FcmApiUrl, _options.ProjectId);
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            // Add OAuth2 bearer token
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync());

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Push notification sent successfully to device {DeviceToken}", 
                    message.DeviceToken.Substring(0, Math.Min(10, message.DeviceToken.Length)) + "...");
                
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                var messageId = result?.ContainsKey("name") == true ? result["name"].ToString() : null;
                
                return NotificationResult.CreateSuccess(null, messageId);
            }

            _logger.LogError("Failed to send push notification. Status: {StatusCode}, Response: {Response}",
                response.StatusCode, responseContent);
            return NotificationResult.CreateFailure($"FCM API error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to user {UserId}", message.UserId);
            return NotificationResult.CreateFailure(ex.Message);
        }
    }

    private async Task<string> GetAccessTokenAsync()
    {
        // In production, implement OAuth2 token retrieval using Google's service account
        // For now, return the server key (legacy method, should migrate to OAuth2)
        return _options.ServerKey;
    }

    private string GetAndroidPriority(NotificationPriority priority)
    {
        return priority switch
        {
            NotificationPriority.Urgent => "high",
            NotificationPriority.High => "high",
            _ => "normal"
        };
    }

    private string GetApnsPriority(NotificationPriority priority)
    {
        return priority switch
        {
            NotificationPriority.Urgent => "10",
            NotificationPriority.High => "10",
            _ => "5"
        };
    }
}

/// <summary>
/// Configuration options for Push Notifications (FCM)
/// </summary>
public class PushNotificationOptions
{
    public const string SectionName = "PushNotification";
    
    /// <summary>
    /// Firebase project ID
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;
    
    /// <summary>
    /// Firebase server key (legacy) or service account key
    /// </summary>
    public string ServerKey { get; set; } = string.Empty;
    
    /// <summary>
    /// API timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
