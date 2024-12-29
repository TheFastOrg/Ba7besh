using Asp.Versioning;
using Ba7besh.Api.Helpers;
using Ba7besh.Application.DeviceManagement;
using Microsoft.AspNetCore.Mvc;
using Paramore.Brighter;

namespace Ba7besh.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/devices")]
public class DevicesController(IAmACommandProcessor commandProcessor) : ControllerBase
{
    [HttpPost("register")]
    [SkipDeviceValidation]
    public async Task<ActionResult<DeviceRegistrationResult>> Register([FromBody] RegisterDeviceRequest request)
    {
        var command = new RegisterDeviceCommand(request.AppVersion);
        await commandProcessor.SendAsync(command);
        return Ok(command.Response);
    }
}

public record RegisterDeviceRequest(string AppVersion);