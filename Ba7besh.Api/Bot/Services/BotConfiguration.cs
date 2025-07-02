namespace Ba7besh.Api.Bot.Services;

public class BotConfiguration
{
    public static readonly string ConfigSection = "BotConfiguration";
    
    public string BotToken { get; set; } = string.Empty;
}