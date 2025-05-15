using Ba7besh.Api.Helpers;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Host.ConfigureLogging();

var botApiToken = builder.Configuration["BotApi:AuthToken"];
Console.WriteLine($"Bot API Token configured: {(!string.IsNullOrEmpty(botApiToken) ? "Yes" : "No")}");


builder.Services
    .AddSingleton<DiagnosticContext>()
    .AddBa7beshInfrastructure(builder.Configuration)
    .AddBa7beshAuthentication()
    .AddBa7beshCQRS()
    .AddBa7beshApi();

var app = builder.Build();

app.UseDevEnvFeatures(app.Environment);
app.MapHealthChecks("/health").WithMetadata(new SkipDeviceValidationAttribute());
app.UseBa7beshExceptionHandler();
app.UseBa7beshPipeline();
app.MapControllers();
app.Use(async (context, next) =>
{
    // Skip authentication for certain paths
    if (context.Request.Path.StartsWithSegments("/health") || 
        context.Request.Path.StartsWithSegments("/robots933456.txt") ||
        context.Request.Path.StartsWithSegments("/swagger"))
    {
        await next();
        return;
    }
    
    // Check for API token in either Authorization header or custom header
    string token = null;
    
    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        var authHeaderValue = authHeader.ToString();
        if (authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authHeaderValue.Substring("Bearer ".Length).Trim();
        }
    }
    
    if (token == null && context.Request.Headers.TryGetValue("X-Bot-Api-Key", out var apiKeyHeader))
    {
        token = apiKeyHeader.ToString();
    }
    
    // If no token was provided or it doesn't match, return 401
    if (string.IsNullOrEmpty(token) || token != botApiToken)
    {
        Console.WriteLine("Authentication failed: " + (string.IsNullOrEmpty(token) ? "No token provided" : "Invalid token"));
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { error = "Authentication failed" });
        return;
    }
    
    // Authentication successful, proceed with the request
    Console.WriteLine("Authentication successful");
    await next();
});

app.Run();