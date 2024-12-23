using Paramore.Brighter;

namespace Ba7besh.Application.Authentication;

public class GoogleAuthCommand(string idToken) : Command(Guid.NewGuid())
{
    public string IdToken { get; } = idToken;
}