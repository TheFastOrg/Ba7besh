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

app.Run();