namespace Ba7besh.Application.ReviewManagement;

public record GetBusinessReviewsResult
{
    public IReadOnlyList<ReviewSummary> Reviews { get; init; } = Array.Empty<ReviewSummary>();
    public int TotalCount { get; init; }
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
}