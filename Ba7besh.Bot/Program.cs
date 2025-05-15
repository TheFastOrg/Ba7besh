using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ba7besh.Bot.Services;

namespace Ba7besh.Bot;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Create and configure the host
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register configuration
                services.Configure<BotConfiguration>(
                    context.Configuration.GetSection(BotConfiguration.ConfigSection));
                
                // Register API client
                services.AddHttpClient<IBa7beshApiClient, Ba7beshApiClient>(client =>
                {
                    client.BaseAddress = new Uri(context.Configuration["Api:BaseUrl"]);
                });
                
                // Register conversation service
                services.AddSingleton<ConversationService>();
                
                // Register the Bot service
                services.AddHostedService<TelegramBotService>();
            })
            .Build();

        await host.RunAsync();
    }
}