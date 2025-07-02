using Ba7besh.Application.Authentication;
using Paramore.Brighter;

namespace Ba7besh.Api.Bot.Services;

public class TelegramUserAuthProvider(
    IAmACommandProcessor commandProcessor,
    ILogger<TelegramUserAuthProvider> logger)
{
    public string GenerateUserId(long chatId)
    {
        return $"telegram_{chatId}";
    }
    
    public async Task<AuthResult> AuthenticateUserAsync(long chatId, string firstName, string lastName, string? username)
    {
        try
        {
            var userId = GenerateUserId(chatId);
        
            var command = new TelegramAuthCommand(chatId, firstName, lastName, username);
            await commandProcessor.SendAsync(command);
        
            return new AuthResult
            {
                Success = true,
                UserId = userId,
                BackendToken = command.Response?.Token,
                TelegramId = chatId,
                FirstName = firstName,
                LastName = lastName,
                Username = username
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error authenticating user {ChatId}", chatId);
        
            return new AuthResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
public class AuthResult
{
    public bool Success { get; set; }
    public string? UserId { get; set; }
    public string? BackendToken { get; set; }
    public long TelegramId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? ErrorMessage { get; set; }
}