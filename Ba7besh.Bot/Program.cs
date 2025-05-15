using Ba7besh.Bot.Services;

namespace Ba7besh.Bot;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Register your services
        builder.Services.Configure<BotConfiguration>(
            builder.Configuration.GetSection(BotConfiguration.ConfigSection));
        
        builder.Services.AddHttpClient<IBa7beshApiClient, Ba7beshApiClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]);
        });
        
        builder.Services.AddSingleton<ConversationService>();
        builder.Services.AddHostedService<TelegramBotService>();
        
        var app = builder.Build();
        
        // Add a minimal health endpoint
        app.MapGet("/health", () => "Healthy");
        
        await app.RunAsync();
    }
}