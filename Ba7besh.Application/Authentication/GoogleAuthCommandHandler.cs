using Paramore.Brighter;

namespace Ba7besh.Application.Authentication;

public class GoogleAuthCommandHandler(IAuthService authService) : RequestHandlerAsync<GoogleAuthCommand>
{
    public override async Task<GoogleAuthCommand> HandleAsync(
        GoogleAuthCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await authService.AuthenticateWithGoogleAsync(command.IdToken);

        if (!result.Success)
            throw new InvalidOperationException("Google authentication failed.");

        return await base.HandleAsync(command, cancellationToken);
    }
}