using Ba7besh.Application.Authentication;

namespace Ba7besh.Api.Helpers;

public static class HttpContextExtensions
{
    private const string AuthenticatedUserKey = "AuthenticatedUser";
    private const string IsBotRequestKey = "IsBotRequest";

    public static void SetAuthenticatedUser(this HttpContext context, AuthenticatedUser user)
    {
        context.Items[AuthenticatedUserKey] = user;
    }

    public static AuthenticatedUser? GetAuthenticatedUser(this HttpContext context)
    {
        return context.Items[AuthenticatedUserKey] as AuthenticatedUser;
    }
    public static bool IsBotRequest(this HttpContext context)
    {
        return context.Items.ContainsKey(IsBotRequestKey) && context.Items[IsBotRequestKey] is true;
    }
}