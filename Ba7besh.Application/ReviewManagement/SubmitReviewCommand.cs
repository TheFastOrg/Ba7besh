using Paramore.Brighter;

namespace Ba7besh.Application.ReviewManagement;

public record ReviewDimensionRating(ReviewDimension Dimension, decimal Rating, string? Note);

public class SubmitReviewCommand() : Command(Guid.NewGuid())
{
    public string BusinessId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal OverallRating { get; set; }
    public string? Content { get; set; }
    public IReadOnlyList<ReviewDimensionRating> DimensionRatings { get; set; } = Array.Empty<ReviewDimensionRating>();
}