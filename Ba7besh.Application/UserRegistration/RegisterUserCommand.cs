using Paramore.Brighter;

namespace Ba7besh.Application.UserRegistration;

public class RegisterUserCommand(string mobileNumber, string password) : Command(Guid.NewGuid())
{
    public string MobileNumber { get; } = mobileNumber;
    public string Password { get; } = password;
}