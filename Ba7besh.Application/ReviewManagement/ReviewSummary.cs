namespace Ba7besh.Application.ReviewManagement;
public record ReviewSummary
{
    public required string Id { get; init; }
    public required string ReviewerName { get; init; }
    public decimal OverallRating { get; set; }
    public string? Content { get; set; }
}