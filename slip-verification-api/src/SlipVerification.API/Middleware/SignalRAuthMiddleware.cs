namespace SlipVerification.API.Middleware;

/// <summary>
/// Middleware to support JWT authentication via query string for SignalR connections
/// </summary>
public class SignalRAuthMiddleware
{
    private readonly RequestDelegate _next;

    public SignalRAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get token from query string (for SignalR)
        if (context.Request.Path.StartsWithSegments("/ws") &&
            context.Request.Query.TryGetValue("access_token", out var token))
        {
            context.Request.Headers.Append("Authorization", $"Bearer {token}");
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method for adding SignalR auth middleware
/// </summary>
public static class SignalRAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseSignalRAuth(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SignalRAuthMiddleware>();
    }
}
