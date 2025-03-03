using Paramore.Brighter;

namespace Ba7besh.Application.Authentication;

public class PhoneAuthCommand(string idToken) : Command(Guid.NewGuid())
{
    public string IdToken { get; } = idToken;
}
public class PhoneAuthCommandHandler(IAuthService authService) : RequestHandlerAsync<PhoneAuthCommand>
{
    public override async Task<PhoneAuthCommand> HandleAsync(
        PhoneAuthCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await authService.AuthenticateWithPhoneAsync(command.IdToken);

        if (!result.Success)
            throw new InvalidOperationException("Phone authentication failed.");

        return await base.HandleAsync(command, cancellationToken);
    }
}