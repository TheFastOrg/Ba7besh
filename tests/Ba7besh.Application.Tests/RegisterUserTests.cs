using Ba7besh.Application.Authentication;
using Moq;

namespace Ba7besh.Application.Tests;

public class RegisterUserTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _handler = new RegisterUserCommandHandler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Should_Register_User_With_Valid_Data()
    {
        // Arrange
        var command = new RegisterUserCommand("+963912345678", "StrongPassword123!");

        _authServiceMock
            .Setup(s => s.RegisterWithMobileAsync(command.MobileNumber, command.Password))
            .ReturnsAsync(new AuthenticationResult
            {
                Success = true,
                UserId = "new-user-id"
            });

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(command, result);
        _authServiceMock.Verify(s => s.RegisterWithMobileAsync(command.MobileNumber, command.Password), Times.Once);
    }


    [Fact]
    public async Task Should_Fail_If_Registration_Fails()
    {
        // Arrange
        var command = new RegisterUserCommand("+963912345678", "StrongPassword123!");

        _authServiceMock
            .Setup(s => s.RegisterWithMobileAsync(command.MobileNumber, command.Password))
            .ReturnsAsync(new AuthenticationResult
            {
                Success = false,
                UserId = null
            });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(command, CancellationToken.None));

        _authServiceMock.Verify(s => s.RegisterWithMobileAsync(command.MobileNumber, command.Password), Times.Once);
    }
}