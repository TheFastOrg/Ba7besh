using Paramore.Brighter;

namespace Ba7besh.Application.Authentication;

public class TelegramAuthCommand(long telegramId, string firstName, string lastName, string? username) 
    : Command(Guid.NewGuid())
{
    public long TelegramId { get; } = telegramId;
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public string? Username { get; } = username;
    public TelegramAuthResponse? Response { get; set; }

}
public record TelegramAuthResponse(string UserId, string Token);

public class TelegramAuthCommandHandler(IAuthService authService) : RequestHandlerAsync<TelegramAuthCommand>
{
    public override async Task<TelegramAuthCommand> HandleAsync(
        TelegramAuthCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await authService.AuthenticateWithTelegramAsync(
            command.TelegramId, 
            command.FirstName, 
            command.LastName, 
            command.Username);

        if (!result.Success)
            throw new InvalidOperationException($"Telegram authentication failed: {result.ErrorMessage}");
        command.Response = new TelegramAuthResponse(result.UserId, result.Token);
        return await base.HandleAsync(command, cancellationToken);
    }
}