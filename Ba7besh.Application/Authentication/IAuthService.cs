namespace Ba7besh.Application.Authentication;

public interface IAuthService
{
    Task<AuthenticationResult> AuthenticateWithGoogleAsync(string idToken);

}