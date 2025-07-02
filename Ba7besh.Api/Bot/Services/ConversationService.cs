using Microsoft.Extensions.Logging;

namespace Ba7besh.Api.Bot.Services;

public enum ConversationStage
{
    None,
    SearchingRestaurant,
    StartingReview,
    AwaitingRestaurantName,
    AwaitingRating,
    AwaitingReviewText,
    AwaitingConfirmation
}

public class ConversationState
{
    public ConversationStage Stage { get; set; } = ConversationStage.None;
    public string? RestaurantId { get; set; }
    public string? RestaurantName { get; set; }
    public decimal? Rating { get; set; }
    public string? ReviewText { get; set; }
    public string? UserId { get; set; }
    public string? BackendToken { get; set; }

    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}

public class ConversationService
{
    private readonly Dictionary<long, ConversationState> _conversations = new();
    private readonly ILogger<ConversationService> _logger;
    
    // Auto-cleanup timer to remove stale conversations
    private readonly Timer _cleanupTimer;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(15);

    public ConversationService(ILogger<ConversationService> logger)
    {
        _logger = logger;
        _cleanupTimer = new Timer(CleanupStaleConversations, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public ConversationState GetOrCreate(long chatId)
    {
        if (!_conversations.TryGetValue(chatId, out var state))
        {
            state = new ConversationState();
            _conversations[chatId] = state;
        }
        
        // Update last activity
        state.LastActivity = DateTime.UtcNow;
        return state;
    }

    public void UpdateState(long chatId, Action<ConversationState> updateAction)
    {
        var state = GetOrCreate(chatId);
        updateAction(state);
    }

    public void Reset(long chatId)
    {
        if (_conversations.ContainsKey(chatId))
        {
            _conversations[chatId] = new ConversationState();
        }
    }
    
    private void CleanupStaleConversations(object? state)
    {
        try
        {
            var now = DateTime.UtcNow;
            var staleChats = _conversations
                .Where(kvp => now - kvp.Value.LastActivity > _timeout)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var chatId in staleChats)
            {
                _conversations.Remove(chatId);
            }
            
            if (staleChats.Count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} stale conversations", staleChats.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up stale conversations");
        }
    }
}