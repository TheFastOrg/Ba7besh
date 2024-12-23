using Asp.Versioning;
using Ba7besh.Application.Authentication;
using Microsoft.AspNetCore.Mvc;
using Paramore.Brighter;

namespace Ba7besh.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(IAmACommandProcessor commandProcessor) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var command = new RegisterUserCommand(request.MobileNumber, request.Password);

        await commandProcessor.SendAsync(command);
        return Ok("User registered successfully!");
    }
    
    [HttpPost("google")]
    public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthRequest request)
    {
        var command = new GoogleAuthCommand(request.IdToken);
        await commandProcessor.SendAsync(command);
        return Ok("Authentication via google was successful!");
    }
}

public record RegisterUserRequest(string MobileNumber, string Password);
public record GoogleAuthRequest(string IdToken);
