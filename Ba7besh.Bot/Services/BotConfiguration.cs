namespace Ba7besh.Bot.Services;

public class BotConfiguration
{
    public static readonly string ConfigSection = "BotConfiguration";
    
    public string BotToken { get; set; } = string.Empty;
    public string HostAddress { get; set; } = string.Empty;
}