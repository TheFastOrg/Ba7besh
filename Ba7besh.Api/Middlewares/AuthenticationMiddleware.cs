using Ba7besh.Api.Helpers;
using Ba7besh.Application.Authentication;

namespace Ba7besh.Api.Middlewares;

public class AuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader.Substring(7);
            try
            {
                var user = await authService.VerifyTokenAsync(token);
                context.SetAuthenticatedUser(user);
            }
            catch
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or expired token.");
                return;
            }
        }

        await next(context);
    }
}