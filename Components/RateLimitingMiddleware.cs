using System.Collections.Concurrent;
using System.Net;

namespace LC360.Components;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    // IP -> (attempt count, window start time)
    private static readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)>
        _ipAttempts = new();

    private const int MaxAttempts = 10;
    private const int WindowMinutes = 1;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only rate limit the login endpoint
        if (!context.Request.Path.StartsWithSegments("/api/auth/login"))
        {
            await _next(context);
            return;
        }

        var ip = GetClientIp(context);
        var now = DateTime.UtcNow;

        var entry = _ipAttempts.AddOrUpdate(
            ip,
            _ => (1, now),
            (_, existing) =>
            {
                // Reset window if expired
                if ((now - existing.WindowStart).TotalMinutes >= WindowMinutes)
                    return (1, now);
                return (existing.Count + 1, existing.WindowStart);
            }
        );

        if (entry.Count > MaxAttempts)
        {
            _logger.LogWarning("Rate limit exceeded for IP: {IP} ({Count} attempts)", ip, entry.Count);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = "60";
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync("""
                {
                    "error": "Too many login attempts. Please wait 1 minute before trying again.",
                    "retryAfterSeconds": 60
                }
            """);
            return;
        }

        await _next(context);
    }

    private static string GetClientIp(HttpContext context)
    {
        // Check for proxy headers first
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}