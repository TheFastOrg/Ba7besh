namespace Ba7besh.Bot.Services;

public class TelegramUserAuthProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TelegramUserAuthProvider> _logger;

    public TelegramUserAuthProvider(
        IConfiguration configuration, 
        HttpClient httpClient,
        ILogger<TelegramUserAuthProvider> logger)
    {
        var botApiToken = configuration["Api:AuthToken"]
                           ?? throw new InvalidOperationException("Bot API token is not configured");
        _httpClient = httpClient;
        _logger = logger;

        // Configure HttpClient for backend API calls
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", botApiToken);
    }

    public string GenerateUserId(long chatId)
    {
        return $"telegram_{chatId}";
    }
    
    public async Task<AuthResult> AuthenticateUserAsync(long chatId, string firstName, string lastName, string? username)
    {
        try
        {
            var userId = GenerateUserId(chatId);
        
            // Create auth request for backend
            var authRequest = new TelegramAuthRequest
            {
                TelegramId = chatId,
                FirstName = firstName,
                LastName = lastName,
                Username = username
            };

            // Call backend auth endpoint
            var response = await _httpClient.PostAsJsonAsync("/auth/telegram", authRequest);
        
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<TelegramAuthResponse>();
    
                return new AuthResult
                {
                    Success = true,
                    UserId = userId,
                    BackendToken = authResponse?.Token,
                    TelegramId = chatId,
                    FirstName = firstName,
                    LastName = lastName,
                    Username = username
                };
            }
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Backend authentication failed: {StatusCode} - {Error}", 
                response.StatusCode, errorContent);
            
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"Backend auth failed: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user {ChatId}", chatId);
        
            return new AuthResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}

public class TelegramAuthRequest
{
    public long TelegramId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Username { get; set; }
}

public class TelegramAuthResponse
{
    public string? Token { get; set; }
    public string? UserId { get; set; }
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? UserId { get; set; }
    public string? BackendToken { get; set; }
    public long TelegramId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? ErrorMessage { get; set; }
}