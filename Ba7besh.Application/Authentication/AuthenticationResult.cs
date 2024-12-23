namespace Ba7besh.Application.Authentication;

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string? UserId { get; set; }
    public string? Token { get; set; }
    public bool IsNewUser { get; set; }
    public string? ErrorMessage { get; set; }
}