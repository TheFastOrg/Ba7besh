using System.Data;
using System.Security.Cryptography;
using System.Text;
using Dapper;

namespace Ba7besh.Application.DeviceManagement;

public class SignatureValidationService(IDbConnection db)
{
    private static readonly TimeSpan TimestampValidityWindow = TimeSpan.FromMinutes(5);

    public async Task<bool> ValidateRequestAsync(string deviceId, string signature, string timestamp, string path,
        string? body = null)
    {
        if (!DateTime.TryParse(timestamp, out var requestTime))
            return false;

        if (Math.Abs((DateTime.UtcNow - requestTime).TotalMinutes) > TimestampValidityWindow.TotalMinutes)
            return false;

        var device = await db.QuerySingleOrDefaultAsync<DeviceInfo>("""
                                                                    SELECT device_signature_key, is_banned, is_deleted
                                                                    FROM registered_devices 
                                                                    WHERE id = @DeviceId
                                                                    """,
            new { DeviceId = deviceId });

        if (device == null || device.IsDeleted || device.IsBanned)
            return false;

        var computedSignature = ComputeSignature(device.DeviceSignatureKey, timestamp, path, body);
        return signature == computedSignature;
    }

    private static string ComputeSignature(string key, string timestamp, string path, string? body)
    {
        var signatureContent = $"{timestamp}|{path}";
        if (!string.IsNullOrEmpty(body))
            signatureContent += $"|{body}";

        using var hmac = new HMACSHA256(Convert.FromBase64String(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureContent));
        return Convert.ToBase64String(hash);
    }

    private record DeviceInfo(string DeviceSignatureKey, bool IsBanned, bool IsDeleted);
}