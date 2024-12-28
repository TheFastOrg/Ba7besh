using System.Data;
using System.Security.Cryptography;
using Dapper;
using Paramore.Brighter;

namespace Ba7besh.Application.DeviceManagement;

public class RegisterDeviceCommandHandler(IDbConnection db) : RequestHandlerAsync<RegisterDeviceCommand>
{
    private const int SignatureKeyLengthInBytes = 32;

    public override async Task<RegisterDeviceCommand> HandleAsync(
        RegisterDeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        var deviceId = Guid.NewGuid().ToString("N");
        var signatureKey = GenerateSignatureKey();
        var now = DateTime.UtcNow;

        await db.ExecuteAsync("""
                              INSERT INTO registered_devices 
                              (id, app_version, device_signature_key, created_at, last_used_at)
                              VALUES 
                              (@DeviceId, @AppVersion, @SignatureKey, @Now, @Now)
                              """,
            new
            {
                DeviceId = deviceId,
                command.AppVersion,
                SignatureKey = signatureKey,
                Now = now
            });

        command.Response = new DeviceRegistrationResult(
            deviceId,
            signatureKey,
            now.AddMonths(3));

        return command;
    }

    private static string GenerateSignatureKey()
    {
        var keyBytes = new byte[SignatureKeyLengthInBytes];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(keyBytes);
        return Convert.ToBase64String(keyBytes);
    }
}