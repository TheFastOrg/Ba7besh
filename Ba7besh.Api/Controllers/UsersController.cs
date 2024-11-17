using Asp.Versioning;
using Ba7besh.Application.UserRegistration;
using Microsoft.AspNetCore.Mvc;
using Paramore.Brighter;

namespace Ba7besh.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController(IAmACommandProcessor commandProcessor) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var command = new RegisterUserCommand(request.MobileNumber, request.Password);

        await commandProcessor.SendAsync(command);
        return Ok("User registered successfully!");
    }
}

public record RegisterUserRequest(string MobileNumber, string Password);