using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Ba7besh.Api.Helpers;

public static class LoggingExtensions
{
    public static IHostBuilder ConfigureLogging(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, services, configuration) =>
        {
            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console();

            if (context.HostingEnvironment.IsDevelopment())
            {
                configuration.WriteTo.File(
                    new CompactJsonFormatter(),
                    "logs/ba7besh.json",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7);
            }
            else
            {
                var connectionString = context.Configuration["Storage:ConnectionString"];
                var containerName = context.Configuration["Storage:ContainerName"];
                
                configuration.WriteTo.AzureBlobStorage(
                    connectionString:connectionString,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    storageContainerName: containerName,
                    storageFileName: "{yyyy}/{MM}/{dd}/ba7besh.json",
                    period: TimeSpan.FromSeconds(30),
                    batchPostingLimit: 50,
                    formatter: new CompactJsonFormatter());
            }
        });
    }
}