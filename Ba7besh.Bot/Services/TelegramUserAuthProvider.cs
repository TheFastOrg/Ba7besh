using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Ba7besh.Bot.Services;

public class TelegramUserAuthProvider(IConfiguration configuration)
{
    private readonly string _secretKey = configuration["BotConfiguration:SecretKey"] 
                                         ?? throw new InvalidOperationException("Bot secret key is not configured");

    public string GenerateUserId(long chatId)
    {
        // Use a consistent way to generate user IDs for Telegram users
        return $"telegram_{chatId}";
    }
    
    public string GenerateAuthToken(long chatId)
    {
        // Create a simple token for this user
        var data = $"{chatId}_{DateTime.UtcNow.Ticks}_{_secretKey}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hashBytes);
    }
}