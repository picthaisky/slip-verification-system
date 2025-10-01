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
/// SMS channel implementation using Twilio
/// </summary>
public class SmsChannel : INotificationChannel
{
    private readonly ILogger<SmsChannel> _logger;
    private readonly SmsOptions _options;
    private readonly HttpClient _httpClient;

    public NotificationChannel ChannelType => NotificationChannel.SMS;

    public SmsChannel(
        ILogger<SmsChannel> logger,
        IOptions<SmsOptions> options,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClientFactory.CreateClient("Twilio");
    }

    public bool SupportsChannel(NotificationChannel channel)
    {
        return channel == NotificationChannel.SMS;
    }

    public async Task<NotificationResult> SendAsync(NotificationMessage message)
    {
        try
        {
            if (string.IsNullOrEmpty(message.RecipientPhone))
            {
                return NotificationResult.CreateFailure("Recipient phone number is required");
            }

            // Format phone number to E.164 format if needed
            var phoneNumber = FormatPhoneNumber(message.RecipientPhone);
            
            // Combine title and message for SMS (160 chars limit consideration)
            var smsBody = string.IsNullOrEmpty(message.Title) 
                ? message.Message 
                : $"{message.Title}: {message.Message}";
            
            // Truncate if too long
            if (smsBody.Length > 1600) // Twilio supports up to 1600 chars
            {
                smsBody = smsBody.Substring(0, 1597) + "...";
            }

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("To", phoneNumber),
                new KeyValuePair<string, string>("From", _options.FromNumber),
                new KeyValuePair<string, string>("Body", smsBody)
            });

            var url = $"https://api.twilio.com/2010-04-01/Accounts/{_options.AccountSid}/Messages.json";
            
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            // Add Basic Authentication
            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_options.AccountSid}:{_options.AuthToken}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}", MaskPhoneNumber(phoneNumber));
                
                var result = JsonSerializer.Deserialize<TwilioSmsResponse>(responseContent);
                return NotificationResult.CreateSuccess(null, result?.Sid);
            }

            _logger.LogError("Failed to send SMS. Status: {StatusCode}, Response: {Response}",
                response.StatusCode, responseContent);
            return NotificationResult.CreateFailure($"Twilio API error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to user {UserId}", message.UserId);
            return NotificationResult.CreateFailure(ex.Message);
        }
    }

    private string FormatPhoneNumber(string phoneNumber)
    {
        // Remove any non-digit characters
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        
        // If it doesn't start with +, add country code (default to Thailand +66)
        if (!phoneNumber.StartsWith("+"))
        {
            if (digits.StartsWith("0"))
            {
                digits = "66" + digits.Substring(1); // Replace leading 0 with 66 for Thailand
            }
            return "+" + digits;
        }
        
        return phoneNumber;
    }

    private string MaskPhoneNumber(string phoneNumber)
    {
        if (phoneNumber.Length <= 6) return phoneNumber;
        return phoneNumber.Substring(0, 3) + "****" + phoneNumber.Substring(phoneNumber.Length - 3);
    }

    private class TwilioSmsResponse
    {
        public string Sid { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}

/// <summary>
/// Configuration options for SMS (Twilio)
/// </summary>
public class SmsOptions
{
    public const string SectionName = "Sms";
    
    /// <summary>
    /// Twilio Account SID
    /// </summary>
    public string AccountSid { get; set; } = string.Empty;
    
    /// <summary>
    /// Twilio Auth Token
    /// </summary>
    public string AuthToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Twilio phone number (from)
    /// </summary>
    public string FromNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// API timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
