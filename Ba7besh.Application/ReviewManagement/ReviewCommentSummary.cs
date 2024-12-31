namespace Ba7besh.Application.ReviewManagement;

public record ReviewCommentSummary
{
    public required string Id { get; init; }
    public required string UserId { get; init; }
    public required string Content { get; init; }
    public required DateTime CreatedAt { get; init; }
}