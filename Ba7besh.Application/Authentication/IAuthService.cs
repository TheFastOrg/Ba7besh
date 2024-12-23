namespace Ba7besh.Application.Authentication;

public interface IAuthService
{
    Task<AuthenticationResult> RegisterWithMobileAsync(string mobileNumber, string password);
    Task<AuthenticationResult> AuthenticateWithGoogleAsync(string idToken);

}