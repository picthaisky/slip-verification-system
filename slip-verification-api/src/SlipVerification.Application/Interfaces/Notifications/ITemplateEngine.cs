using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Application.Interfaces.Notifications;

/// <summary>
/// Interface for notification template engine
/// </summary>
public interface ITemplateEngine
{
    /// <summary>
    /// Renders a template with the given placeholders
    /// </summary>
    /// <param name="template">The template to render</param>
    /// <param name="placeholders">The placeholder values</param>
    /// <returns>Rendered template string</returns>
    string RenderTemplate(string template, Dictionary<string, string> placeholders);
    
    /// <summary>
    /// Gets a notification template by code and channel
    /// </summary>
    Task<NotificationTemplate?> GetTemplateAsync(string code, NotificationChannel channel, string language = "en");
    
    /// <summary>
    /// Renders a notification template with placeholders
    /// </summary>
    Task<(string subject, string body)> RenderNotificationTemplateAsync(
        string templateCode, 
        NotificationChannel channel, 
        Dictionary<string, string> placeholders,
        string language = "en");
}
