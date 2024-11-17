using Paramore.Brighter;

namespace Ba7besh.Application.UserRegistration;

public class RegisterUserCommandHandler(IRegisterUserService authService) : RequestHandlerAsync<RegisterUserCommand>
{
    public override async Task<RegisterUserCommand> HandleAsync(RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await authService.RegisterAsync(command.MobileNumber, command.Password);

        if (!result.Success)
            throw new InvalidOperationException("Registration failed.");

        return await base.HandleAsync(command, cancellationToken);
    }
}