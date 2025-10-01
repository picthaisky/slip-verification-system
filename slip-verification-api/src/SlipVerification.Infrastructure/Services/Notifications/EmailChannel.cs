using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlipVerification.Application.DTOs.Notifications;
using SlipVerification.Application.Interfaces.Notifications;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Infrastructure.Services.Notifications;

/// <summary>
/// Email channel implementation using SMTP
/// </summary>
public class EmailChannel : INotificationChannel
{
    private readonly ILogger<EmailChannel> _logger;
    private readonly EmailOptions _options;

    public NotificationChannel ChannelType => NotificationChannel.EMAIL;

    public EmailChannel(
        ILogger<EmailChannel> logger,
        IOptions<EmailOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public bool SupportsChannel(NotificationChannel channel)
    {
        return channel == NotificationChannel.EMAIL;
    }

    public async Task<NotificationResult> SendAsync(NotificationMessage message)
    {
        try
        {
            if (string.IsNullOrEmpty(message.RecipientEmail))
            {
                return NotificationResult.CreateFailure("Recipient email is required");
            }

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                Subject = message.Title,
                Body = BuildHtmlBody(message),
                IsBodyHtml = true
            };

            mailMessage.To.Add(message.RecipientEmail);

            // Add attachments if any
            if (!string.IsNullOrEmpty(message.ImagePath) && File.Exists(message.ImagePath))
            {
                mailMessage.Attachments.Add(new Attachment(message.ImagePath));
            }

            using var smtpClient = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                Credentials = new NetworkCredential(_options.Username, _options.Password),
                EnableSsl = _options.EnableSsl
            };

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent successfully to {Email}", message.RecipientEmail);
            return NotificationResult.CreateSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", message.RecipientEmail);
            return NotificationResult.CreateFailure(ex.Message);
        }
    }

    private string BuildHtmlBody(NotificationMessage message)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ padding: 10px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>{message.Title}</h1>
        </div>
        <div class=""content"">
            <p>{message.Message.Replace("\n", "<br>")}</p>
        </div>
        <div class=""footer"">
            <p>© {DateTime.UtcNow.Year} Slip Verification System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}

/// <summary>
/// Configuration options for Email
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Email";
    
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Slip Verification System";
    public bool EnableSsl { get; set; } = true;
}
