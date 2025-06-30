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

    public async Task<AuthenticatedUser> VerifyTokenAsync(string token)
    {
        try
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
            return new AuthenticatedUser(decodedToken.Uid);
        }
        catch (FirebaseAuthException ex)
        {
            throw new UnauthorizedAccessException("Token verification failed.", ex);
        }
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
    public async Task<AuthenticationResult> AuthenticateWithPhoneAsync(string idToken)
    {
        try
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            var uid = decodedToken.Uid;
            var phoneNumber = decodedToken.Claims.TryGetValue("phone_number", out var phone) 
                ? phone.ToString() 
                : null;

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
    public async Task<AuthenticationResult> AuthenticateWithTelegramAsync(long telegramId, string firstName, string lastName, string? username)
    {
        try
        {
            // Create a unique UID for this Telegram user
            var uid = $"telegram_{telegramId}";
        
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

            // Create new user with Telegram data
            var claims = new Dictionary<string, object>
            {
                { "telegram_id", telegramId },
                { "first_name", firstName },
                { "provider", "telegram" }
            };

            if (!string.IsNullOrEmpty(lastName))
                claims["last_name"] = lastName;
        
            if (!string.IsNullOrEmpty(username))
                claims["username"] = username;

            var newUser = await CreateNewUserAsync(uid, claims);
            var newCustomToken = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(uid);
        
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
            DisplayName = claims.GetValueOrDefault("name")?.ToString() 
                          ?? $"{claims.GetValueOrDefault("first_name")} {claims.GetValueOrDefault("last_name")}".Trim(),
            PhotoUrl = claims.GetValueOrDefault("picture")?.ToString()
        };

        return await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
    }
}