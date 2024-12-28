using Ba7besh.Api.Attributes;
using Ba7besh.Application.DeviceManagement;

namespace Ba7besh.Api.Middlewares;

public class DeviceValidationMiddleware(RequestDelegate next, SignatureValidationService signatureValidation, IHostEnvironment hostEnvironment)
{
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