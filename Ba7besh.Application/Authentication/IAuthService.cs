namespace Ba7besh.Application.Authentication;

public interface IAuthService
{
    Task<AuthenticationResult> AuthenticateWithGoogleAsync(string idToken);
    Task<AuthenticationResult> AuthenticateWithPhoneAsync(string idToken);
    Task<AuthenticationResult> AuthenticateWithTelegramAsync(long telegramId, string firstName, string lastName, string? username);

    Task<AuthenticatedUser> VerifyTokenAsync(string token);
}