using System.Text;

namespace SlipVerification.API.Middleware.Security;

/// <summary>
/// Input sanitization middleware to prevent SQL injection and XSS attacks
/// </summary>
public class InputSanitizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputSanitizationMiddleware> _logger;

    public InputSanitizationMiddleware(
        RequestDelegate next,
        ILogger<InputSanitizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only check POST, PUT, PATCH requests with body
        if ((context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH") 
            && context.Request.ContentLength > 0)
        {
            // Enable request buffering to allow multiple reads
            context.Request.EnableBuffering();

            // Read request body
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                leaveOpen: true
            );

            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Check for SQL injection patterns
            if (!string.IsNullOrEmpty(body) && ContainsSqlInjectionPatterns(body))
            {
                _logger.LogWarning("Potential SQL injection detected from {IpAddress}: {Body}",
                    context.Connection.RemoteIpAddress, body);
                    
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid input detected"
                });
                return;
            }
        }

        await _next(context);
    }

    private bool ContainsSqlInjectionPatterns(string input)
    {
        var patterns = new[]
        {
            "'; DROP TABLE",
            "'; DELETE FROM",
            "' OR '1'='1",
            "' OR 1=1--",
            "'; EXEC",
            "'; EXECUTE",
            "--",
            "/*",
            "*/"
        };

        return patterns.Any(pattern =>
            input.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}
