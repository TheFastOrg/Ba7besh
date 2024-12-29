using Ba7besh.Application.Authentication;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace Ba7besh.Infrastructure;

public class FirebaseAuthService : IAuthService
{
    public FirebaseAuthService(string firebaseCredentialsPath)
    {
        if (FirebaseApp.DefaultInstance != null) return;
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(firebaseCredentialsPath)
        });
    }

    public async Task<AuthenticationResult> AuthenticateWithGoogleAsync(string idToken)
    {
        try
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            var uid = decodedToken.Uid;

            var existingUser = await GetUserAsync(uid);
            if (existingUser != null)
            {
                var customToken = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(uid);
                return new AuthenticationResult
                {
                    Success = true,
                    UserId = existingUser.Uid,
                    Token = customToken,
                    IsNewUser = false
                };
            }

            var newUser = await CreateNewUserAsync(uid, decodedToken.Claims);
            var newCustomToken = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(newUser.Uid);
            return new AuthenticationResult
            {
                Success = true,
                UserId = newUser.Uid,
                Token = newCustomToken,
                IsNewUser = true
            };
        }
        catch (FirebaseAuthException ex)
        {
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static async Task<UserRecord?> GetUserAsync(string uid)
    {
        try
        {
            return await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
        }
        catch (FirebaseAuthException)
        {
            return null;
        }
    }

    private static async Task<UserRecord> CreateNewUserAsync(string uid, IReadOnlyDictionary<string, object> claims)
    {
        var userArgs = new UserRecordArgs
        {
            Uid = uid,
            Email = claims.GetValueOrDefault("email")?.ToString(),
            DisplayName = claims.GetValueOrDefault("name")?.ToString(),
            PhotoUrl = claims.GetValueOrDefault("picture")?.ToString()
        };

        return await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
    }
}