using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SlipVerification.Application.Configuration;

namespace SlipVerification.API.Middleware.Security;

/// <summary>
/// Rate limiting middleware to prevent abuse
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly RateLimitOptions _options;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        IOptions<RateLimitOptions> options,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var endpoint = context.Request.Path.ToString();
        var cacheKey = $"rate_limit_{clientIp}_{endpoint}";

        if (_cache.TryGetValue(cacheKey, out int requestCount))
        {
            if (requestCount >= _options.MaxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for {ClientIp} on {Endpoint}", clientIp, endpoint);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests. Please try again later.",
                    retryAfter = _options.TimeWindow.TotalSeconds
                });
                return;
            }

            _cache.Set(cacheKey, requestCount + 1, _options.TimeWindow);
        }
        else
        {
            _cache.Set(cacheKey, 1, _options.TimeWindow);
        }

        await _next(context);
    }
}
