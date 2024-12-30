namespace Ba7besh.Application.ReviewManagement;

public record ReviewReactionsSummary
{
    public required int HelpfulCount { get; init; }
    public required int UnhelpfulCount { get; init; }
    public ReviewReaction? UserReaction { get; init; }
}