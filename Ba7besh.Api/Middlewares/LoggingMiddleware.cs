using System.Diagnostics;
using System.Security.Claims;
using Ba7besh.Api.Helpers;
using Serilog.Events;

namespace Ba7besh.Api.Middlewares;

public class LoggingMiddleware(
    RequestDelegate next,
    ILogger<LoggingMiddleware> logger,
    DiagnosticContext diagnosticContext)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var start = DateTime.UtcNow;
        var sw = Stopwatch.StartNew();

        try
        {
            await next(context);
            sw.Stop();

            var statusCode = context.Response?.StatusCode;
            var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;
            if (level == LogEventLevel.Error)
                LogForErrorResponse(context, statusCode, sw.Elapsed, start);
            else
                LogForSuccessResponse(context, statusCode, sw.Elapsed, start);
        }
        catch (Exception ex)
        {
            sw.Stop();
            LogUnhandledException(context, ex, sw.Elapsed, start);
            throw;
        }
    }

    private void LogForSuccessResponse(HttpContext context, int? statusCode, TimeSpan elapsed, DateTime start)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        diagnosticContext.Set("UserId", userId);
        diagnosticContext.Set("StartTime", start);
        diagnosticContext.Set("Elapsed", elapsed.TotalMilliseconds);

        logger.LogInformation(
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms",
            context.Request.Method,
            context.Request.Path,
            statusCode,
            elapsed.TotalMilliseconds);
    }

    private void LogForErrorResponse(HttpContext context, int? statusCode, TimeSpan elapsed, DateTime start)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        diagnosticContext.Set("UserId", userId);
        diagnosticContext.Set("StartTime", start);
        diagnosticContext.Set("Elapsed", elapsed.TotalMilliseconds);

        logger.LogError(
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms",
            context.Request.Method,
            context.Request.Path,
            statusCode,
            elapsed.TotalMilliseconds);
    }

    private void LogUnhandledException(
        HttpContext context,
        Exception exception,
        TimeSpan elapsed,
        DateTime start)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        diagnosticContext.Set("UserId", userId);
        diagnosticContext.Set("StartTime", start);
        diagnosticContext.Set("Elapsed", elapsed.TotalMilliseconds);

        logger.LogError(
            exception,
            "HTTP {RequestMethod} {RequestPath} failed in {Elapsed:0.0000} ms",
            context.Request.Method,
            context.Request.Path,
            elapsed.TotalMilliseconds);
    }
}