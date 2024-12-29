using Ba7besh.Application.Authentication;

namespace Ba7besh.Api.Helpers;

public static class HttpContextExtensions
{
    private const string AuthenticatedUserKey = "AuthenticatedUser";

    public static void SetAuthenticatedUser(this HttpContext context, AuthenticatedUser user)
    {
        context.Items[AuthenticatedUserKey] = user;
    }

    public static AuthenticatedUser? GetAuthenticatedUser(this HttpContext context)
    {
        return context.Items[AuthenticatedUserKey] as AuthenticatedUser;
    }
}