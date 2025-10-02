using System.Diagnostics;
using SlipVerification.API.Services;

namespace SlipVerification.API.Middleware;

/// <summary>
/// Middleware for automatic request metrics collection
/// </summary>
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MetricsMiddleware> _logger;

    public MetricsMiddleware(RequestDelegate next, ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IMetrics metrics)
    {
        // Skip metrics endpoint itself to avoid recursion
        if (context.Request.Path.StartsWithSegments("/metrics"))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path.ToString();

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            metrics.RecordError(ex.GetType().Name, path);
            _logger.LogError(ex, "Error processing request {Method} {Path}", method, path);
            throw;
        }
        finally
        {
            sw.Stop();
            var statusCode = context.Response.StatusCode;
            var durationSeconds = sw.Elapsed.TotalSeconds;

            metrics.RecordRequestDuration(method, path, statusCode, durationSeconds);

            // Log slow requests
            if (durationSeconds > 1)
            {
                _logger.LogWarning(
                    "Slow request detected: {Method} {Path} took {Duration}ms (Status: {StatusCode})",
                    method, path, sw.ElapsedMilliseconds, statusCode);
            }
        }
    }
}
