using Ba7besh.Application.DeviceManagement;
using Dapper;

namespace Ba7besh.Application.Tests;

public class RegisterDeviceTests : DatabaseTestBase
{
    private readonly RegisterDeviceCommandHandler _handler;
    private const string TestAppVersion = "1.0.0";

    public RegisterDeviceTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _handler = new RegisterDeviceCommandHandler(Connection);
    }

    [Fact]
    public async Task Should_Register_Device_Successfully()
    {
        var command = new RegisterDeviceCommand(TestAppVersion);

        await _handler.HandleAsync(command);

        var device = await Connection.QuerySingleAsync(
            """
            SELECT id, app_version, device_signature_key 
            FROM registered_devices 
            WHERE app_version = @AppVersion
            """,
            new { AppVersion = TestAppVersion });

        Assert.NotNull(device);
        Assert.NotNull(command.Response);
        Assert.Equal(TestAppVersion, device.app_version);
        Assert.Equal(command.Response.DeviceId, device.id);
        Assert.Equal(command.Response.SignatureKey, device.device_signature_key);
        Assert.True(command.Response.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Should_Generate_Unique_Device_Id()
    {
        // Arrange
        var command1 = new RegisterDeviceCommand(TestAppVersion);
        var command2 = new RegisterDeviceCommand(TestAppVersion);

        // Act
        await _handler.HandleAsync(command1);
        await _handler.HandleAsync(command2);

        // Assert
        Assert.NotEqual(command1.Response?.DeviceId, command2.Response?.DeviceId);
    }

    [Fact]
    public async Task Should_Generate_Unique_Signature_Keys()
    {
        // Arrange
        var command1 = new RegisterDeviceCommand(TestAppVersion);
        var command2 = new RegisterDeviceCommand(TestAppVersion);

        // Act
        await _handler.HandleAsync(command1);
        await _handler.HandleAsync(command2);

        // Assert
        Assert.NotEqual(command1.Response?.SignatureKey, command2.Response?.SignatureKey);
    }

    [Fact]
    public async Task Should_Store_Last_Used_At_Timestamp()
    {
        // Arrange
        var command = new RegisterDeviceCommand(TestAppVersion);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var device = await Connection.QuerySingleAsync<DateTime>(
            "SELECT last_used_at FROM registered_devices WHERE app_version = @AppVersion",
            new { AppVersion = TestAppVersion });

        var timeDifference = DateTime.UtcNow - device;
        Assert.True(timeDifference.TotalSeconds < 5);
    }
}