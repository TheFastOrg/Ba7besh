using Ba7besh.Bot.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.Configure<BotConfiguration>(
    builder.Configuration.GetSection(BotConfiguration.ConfigSection));

builder.Services.AddHttpClient<IBa7beshApiClient, Ba7beshApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]);
});

builder.Services.AddSingleton<ConversationService>();
builder.Services.AddHttpClient<TelegramUserAuthProvider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]);
});
builder.Services.AddHostedService<TelegramBotService>();

var app = builder.Build();

// Minimal web app - just needs to stay alive
app.MapGet("/", () => "Ba7besh Telegram Bot is running!");
app.MapGet("/health", () => "OK");

app.Run();