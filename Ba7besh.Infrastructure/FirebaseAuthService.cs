using Ba7besh.Application.UserRegistration;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace Ba7besh.Infrastructure;

public class FirebaseAuthService : IRegisterUserService
{
    public FirebaseAuthService(string firebaseCredentialsPath)
    {
        if (FirebaseApp.DefaultInstance != null) return;
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(firebaseCredentialsPath)
        });
    }

    public async Task<UserRegistrationResult> RegisterAsync(string mobileNumber, string password)
    {
        var userRecordArgs = new UserRecordArgs
        {
            PhoneNumber = mobileNumber,
            Password = password
        };

        try
        {
            var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userRecordArgs);

            return new UserRegistrationResult
            {
                Success = true,
                UserId = userRecord.Uid
            };
        }
        catch (Exception ex)
        {
            return new UserRegistrationResult
            {
                Success = false,
                UserId = null
            };
        }
    }
}