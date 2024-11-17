namespace Ba7besh.Application.UserRegistration;

public interface IRegisterUserService
{
    Task<UserRegistrationResult> RegisterAsync(string mobileNumber, string password);
}