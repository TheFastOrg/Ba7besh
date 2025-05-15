using Ba7besh.Api.Helpers;
using Ba7besh.Application.DeviceManagement;

namespace Ba7besh.Api.Middlewares;

public class DeviceValidationMiddleware(
    RequestDelegate next,
    SignatureValidationService signatureValidation,
    IHostEnvironment hostEnvironment,
    IConfiguration configuration,
    ILogger<DeviceValidationMiddleware> logger)
{
    private readonly string _botApiToken = configuration["BotApi:AuthToken"];

    public async Task InvokeAsync(HttpContext context)
    {
        if (hostEnvironment.IsDevelopment())
        {
            await next(context);
            return;
        }
        
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<SkipDeviceValidationAttribute>() != null)
        {
            await next(context);
            return;
        }
        
        // Check if this is a bot API request (has bot token)
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var authHeaderValue = authHeader.ToString();
            if (authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeaderValue.Substring("Bearer ".Length).Trim();
                
                // If it's the bot token, skip validation
                if (!string.IsNullOrEmpty(_botApiToken) && token == _botApiToken)
                {
                    logger.LogInformation("Bot authentication detected - skipping device validation");
                    await next(context);
                    return;
                }
            }
        }
        
        // Also check for X-Bot-Api-Key header as fallback
        if (context.Request.Headers.TryGetValue("X-Bot-Api-Key", out var apiKeyHeader))
        {
            // If it's the bot token, skip validation
            if (!string.IsNullOrEmpty(_botApiToken) && apiKeyHeader == _botApiToken)
            {
                logger.LogInformation("Bot API key detected - skipping device validation");
                await next(context);
                return;
            }
        }

        if (!TryGetHeaderValues(context.Request.Headers, out var deviceId, out var signature, out var timestamp))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        string? body = null;
        if (context.Request.ContentLength > 0)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var isValid = await signatureValidation.ValidateRequestAsync(
            deviceId, 
            signature, 
            timestamp,
            context.Request.Path,
            body);

        if (!isValid)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await next(context);
    }

    private static bool TryGetHeaderValues(
        IHeaderDictionary headers,
        out string deviceId,
        out string signature,
        out string timestamp)
    {
        deviceId = headers["X-Device-ID"].ToString();
        signature = headers["X-Signature"].ToString();
        timestamp = headers["X-Timestamp"].ToString();

        return !string.IsNullOrEmpty(deviceId) 
               && !string.IsNullOrEmpty(signature)
               && !string.IsNullOrEmpty(timestamp);
    }
}