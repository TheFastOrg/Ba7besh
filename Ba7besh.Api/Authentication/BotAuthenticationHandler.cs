using System.Security.Claims;
using System.Text.Encodings.Web;
using Ba7besh.Application.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Ba7besh.Api.Authentication;

public class BotAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ISystemClock clock,
    IConfiguration configuration)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder, clock)
{
    private readonly string _botApiToken = configuration["BotApi:AuthToken"];

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if authorization header exists
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return Task.FromResult(AuthenticateResult.Fail("Authorization header not found"));

        var authHeaderValue = authHeader.ToString();
    
        // Check if it's a Bearer token
        if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.Fail("Bearer token not found"));

        // Extract the token
        var token = authHeaderValue.Substring("Bearer ".Length).Trim();
    
        // Validate the token (simple comparison for bot token)
        if (string.IsNullOrEmpty(_botApiToken) || token != _botApiToken)
            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));

        // Create authenticated user
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "bot-service"),
            new Claim(ClaimTypes.Name, "Ba7besh Bot"),
            new Claim(ClaimTypes.Role, "BotService")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        // Create an AuthenticatedUser instance and set it in the HttpContext
        var authenticatedUser = new AuthenticatedUser("bot-service");
        Context.Items["AuthenticatedUser"] = authenticatedUser;
    
        // Set a flag to indicate this is a bot-authenticated request
        Context.Items["IsBotRequest"] = true;

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}