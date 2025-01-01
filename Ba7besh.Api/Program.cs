using Ba7besh.Api.Helpers;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Host.ConfigureLogging();

builder.Services
    .AddSingleton<DiagnosticContext>()
    .AddBa7beshInfrastructure(builder.Configuration)
    .AddBa7beshAuthentication()
    .AddBa7beshCQRS()
    .AddBa7beshApi();

var app = builder.Build();

app.UseDevEnvFeatures(app.Environment);
app.MapHealthChecks("/health");
app.UseBa7beshExceptionHandler();
app.UseBa7beshPipeline();
app.MapControllers();

app.Run();