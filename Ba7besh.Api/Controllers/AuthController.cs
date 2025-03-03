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
    
    [HttpPost("google")]
    public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthRequest request)
    {
        var command = new GoogleAuthCommand(request.IdToken);
        await commandProcessor.SendAsync(command);
        return Ok("Authentication via google was successful!");
    }
    [HttpPost("phone")]
    public async Task<IActionResult> PhoneAuth([FromBody] PhoneAuthRequest request)
    {
        var command = new PhoneAuthCommand(request.IdToken);
        await commandProcessor.SendAsync(command);
        return Ok("Authentication via phone was successful!");
    }
}

public record GoogleAuthRequest(string IdToken);
public record PhoneAuthRequest(string IdToken);
