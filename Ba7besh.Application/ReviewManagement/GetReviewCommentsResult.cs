namespace Ba7besh.Application.ReviewManagement;

public record GetReviewCommentsResult
{
    public IReadOnlyList<ReviewCommentSummary> Comments { get; init; } = Array.Empty<ReviewCommentSummary>();
    public int TotalCount { get; init; }
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
}