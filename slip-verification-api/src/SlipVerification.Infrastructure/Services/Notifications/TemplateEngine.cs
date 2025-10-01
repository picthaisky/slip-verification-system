using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SlipVerification.Application.Interfaces.Notifications;
using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;
using SlipVerification.Infrastructure.Data;

namespace SlipVerification.Infrastructure.Services.Notifications;

/// <summary>
/// Template engine for rendering notification templates
/// </summary>
public class TemplateEngine : ITemplateEngine
{
    private readonly ILogger<TemplateEngine> _logger;
    private readonly ApplicationDbContext _context;
    private static readonly Regex PlaceholderRegex = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

    public TemplateEngine(
        ILogger<TemplateEngine> logger,
        ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public string RenderTemplate(string template, Dictionary<string, string> placeholders)
    {
        if (string.IsNullOrEmpty(template) || placeholders == null || !placeholders.Any())
        {
            return template;
        }

        return PlaceholderRegex.Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return placeholders.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

    public async Task<NotificationTemplate?> GetTemplateAsync(string code, NotificationChannel channel, string language = "en")
    {
        try
        {
            var template = await _context.Set<NotificationTemplate>()
                .Where(t => t.Code == code 
                    && t.Channel == channel 
                    && t.Language == language
                    && t.IsActive
                    && !t.IsDeleted)
                .FirstOrDefaultAsync();

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template {Code} for channel {Channel} and language {Language}",
                code, channel, language);
            return null;
        }
    }

    public async Task<(string subject, string body)> RenderNotificationTemplateAsync(
        string templateCode,
        NotificationChannel channel,
        Dictionary<string, string> placeholders,
        string language = "en")
    {
        try
        {
            var template = await GetTemplateAsync(templateCode, channel, language);

            if (template == null)
            {
                _logger.LogWarning("Template {Code} not found for channel {Channel} and language {Language}",
                    templateCode, channel, language);
                return (string.Empty, string.Empty);
            }

            var renderedSubject = RenderTemplate(template.Subject, placeholders);
            var renderedBody = RenderTemplate(template.Body, placeholders);

            _logger.LogDebug("Template {Code} rendered successfully", templateCode);

            return (renderedSubject, renderedBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template {Code}", templateCode);
            return (string.Empty, string.Empty);
        }
    }
}
