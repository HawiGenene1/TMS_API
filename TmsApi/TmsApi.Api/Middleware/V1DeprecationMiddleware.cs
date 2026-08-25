namespace TmsApi.Middleware;

public class V1DeprecationMiddleware(RequestDelegate next)
{
    // Pick a date 6+ months from now
    private static readonly DateTimeOffset SunsetDate = new(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

    public async Task InvokeAsync(HttpContext context)
    {
        // This runs AFTER the controller has processed the request
        context.Response.OnStarting(() =>
        {
            // Check if this is a V1 request
            if (context.Request.Path.StartsWithSegments("/api/v1"))
            {
                // Tell clients V1 is deprecated
                context.Response.Headers["Deprecation"] = "true";
                
                // Tell clients when V1 stops working
                context.Response.Headers["Sunset"] = SunsetDate.ToString("R");
                
                // Tell clients where to go instead
                context.Response.Headers["Link"] = $"<{context.Request.Scheme}://{context.Request.Host}/api/v2{context.Request.Path.Value?[7..]}>; rel=\"successor-version\"";
            }
            return Task.CompletedTask;
        });

        await next(context);
    }
}
