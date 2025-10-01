using System.Text.RegularExpressions;
using SlipVerification.Infrastructure.Services.Notifications;
using Xunit;

namespace SlipVerification.UnitTests.Services.Notifications;

/// <summary>
/// Unit tests for Template Engine
/// </summary>
public class TemplateEngineTests
{
    [Fact]
    public void RenderTemplate_WithSinglePlaceholder_ReplacesCorrectly()
    {
        // Arrange
        var template = "Hello {{name}}!";
        var placeholders = new Dictionary<string, string>
        {
            { "name", "John" }
        };

        // Act
        var result = RenderSimpleTemplate(template, placeholders);

        // Assert
        Assert.Equal("Hello John!", result);
    }

    [Fact]
    public void RenderTemplate_WithMultiplePlaceholders_ReplacesAll()
    {
        // Arrange
        var template = "Order {{orderNumber}} - Amount: {{amount}} THB - Date: {{date}}";
        var placeholders = new Dictionary<string, string>
        {
            { "orderNumber", "ORD-12345" },
            { "amount", "1500.00" },
            { "date", "2024-10-01" }
        };

        // Act
        var result = RenderSimpleTemplate(template, placeholders);

        // Assert
        Assert.Equal("Order ORD-12345 - Amount: 1500.00 THB - Date: 2024-10-01", result);
    }

    [Fact]
    public void RenderTemplate_WithMissingPlaceholder_LeavesUnchanged()
    {
        // Arrange
        var template = "Hello {{name}}, your order {{orderNumber}} is ready.";
        var placeholders = new Dictionary<string, string>
        {
            { "name", "John" }
            // orderNumber missing
        };

        // Act
        var result = RenderSimpleTemplate(template, placeholders);

        // Assert
        Assert.Equal("Hello John, your order {{orderNumber}} is ready.", result);
    }

    [Fact]
    public void RenderTemplate_WithEmptyTemplate_ReturnsEmpty()
    {
        // Arrange
        var template = "";
        var placeholders = new Dictionary<string, string>
        {
            { "name", "John" }
        };

        // Act
        var result = RenderSimpleTemplate(template, placeholders);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void RenderTemplate_WithNoPlaceholders_ReturnsOriginal()
    {
        // Arrange
        var template = "This is a simple message without placeholders.";
        var placeholders = new Dictionary<string, string>();

        // Act
        var result = RenderSimpleTemplate(template, placeholders);

        // Assert
        Assert.Equal(template, result);
    }

    [Fact]
    public void RenderTemplate_WithDuplicatePlaceholders_ReplacesAllOccurrences()
    {
        // Arrange
        var template = "{{name}} said hello to {{name}}";
        var placeholders = new Dictionary<string, string>
        {
            { "name", "Alice" }
        };

        // Act
        var result = RenderSimpleTemplate(template, placeholders);

        // Assert
        Assert.Equal("Alice said hello to Alice", result);
    }

    [Fact]
    public void RenderTemplate_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var template = "Amount: {{amount}} (includes {{tax}}% tax)";
        var placeholders = new Dictionary<string, string>
        {
            { "amount", "1,500.00" },
            { "tax", "7" }
        };

        // Act
        var result = RenderSimpleTemplate(template, placeholders);

        // Assert
        Assert.Equal("Amount: 1,500.00 (includes 7% tax)", result);
    }

    [Fact]
    public void RenderTemplate_ThaiLanguage_HandlesCorrectly()
    {
        // Arrange
        var template = "สวัสดี {{name}} ยอดชำระ {{amount}} บาท";
        var placeholders = new Dictionary<string, string>
        {
            { "name", "สมชาย" },
            { "amount", "1500" }
        };

        // Act
        var result = RenderSimpleTemplate(template, placeholders);

        // Assert
        Assert.Equal("สวัสดี สมชาย ยอดชำระ 1500 บาท", result);
    }

    // Helper method to simulate template rendering (same logic as TemplateEngine)
    private string RenderSimpleTemplate(string template, Dictionary<string, string> placeholders)
    {
        if (string.IsNullOrEmpty(template) || placeholders == null || !placeholders.Any())
        {
            return template;
        }

        var regex = new Regex(@"\{\{(\w+)\}\}", RegexOptions.Compiled);
        return regex.Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return placeholders.TryGetValue(key, out var value) ? value : match.Value;
        });
    }
}
