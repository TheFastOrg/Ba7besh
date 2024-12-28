using Paramore.Brighter;

namespace Ba7besh.Application.DeviceManagement;

public class RegisterDeviceCommand(string appVersion) : Command(Guid.NewGuid())
{
    public string AppVersion { get; } = appVersion;
    public DeviceRegistrationResult? Response { get; set; }
}

public record DeviceRegistrationResult(
    string DeviceId,
    string SignatureKey,
    DateTime ExpiresAt);