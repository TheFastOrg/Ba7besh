namespace Ba7besh.Application.ReviewManagement;
public record ReviewSummary
{
    public required string Id { get; init; }
    public required string ReviewerName { get; init; }
    public decimal OverallRating { get; set; }
    public string? Content { get; set; }
    public required IReadOnlyList<ReviewDimensionRating> DimensionRatings { get; init; }
    public required ReviewReactionsSummary ReactionsSummary { get; init; }
    public IReadOnlyList<ReviewPhoto> Photos { get; init; } = Array.Empty<ReviewPhoto>();

}

public record RecentReviewSummary : ReviewSummary
{
    public required string BusinessId { get; init; }
    public required string BusinessArName { get; init; }
    public required string BusinessEnName { get; init; }
    public required string BusinessCity { get; init; }
}