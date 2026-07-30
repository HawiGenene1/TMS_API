using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        // Set header before calling next — once the response starts, headers are locked
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            "Request  [{CorrelationId}] {Method} {Path}",
            correlationId,
            context.Request.Method,
            context.Request.Path);

        await _next(context);

        sw.Stop();

        _logger.LogInformation(
            "Response [{CorrelationId}] {StatusCode} in {ElapsedMs}ms",
            correlationId,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }
}
