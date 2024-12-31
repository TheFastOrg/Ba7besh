using System.Data;
using Ba7besh.Application.Exceptions;
using Ba7besh.Application.ReviewManagement;
using Dapper;

namespace Ba7besh.Application.Helpers;

public static class ReviewHelpers
{
    public static async Task ValidateReviewExists(IDbConnection db, string reviewId)
    {
        var reviewExists = await db.QuerySingleOrDefaultAsync<bool>(
            "SELECT EXISTS(SELECT 1 FROM reviews WHERE id = @Id AND is_deleted = FALSE)",
            new { Id = reviewId });

        if (!reviewExists)
            throw new ReviewNotFoundException(reviewId);
    }
    
    public static ReviewReactionsSummary? MapReactionsSummary(string reactionsJson)
    {
        if (string.IsNullOrEmpty(reactionsJson))
            return null;

        var reactionData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(reactionsJson);
        if (reactionData == null)
            return null;

        return new ReviewReactionsSummary
        {
            HelpfulCount = reactionData.GetValueOrDefault("helpful_count", 0),
            UnhelpfulCount = reactionData.GetValueOrDefault("unhelpful_count", 0)
        };
    }

    public static void AddDimensionRating<TReview>(
        Dictionary<string, TReview> reviewDict,
        string reviewId,
        string? dimension,
        decimal? rating,
        string? note) where TReview : ReviewSummary
    {
        if (string.IsNullOrEmpty(dimension) || rating == null || !reviewDict.TryGetValue(reviewId, out var review))
            return;

        var dimensionRating = new ReviewDimensionRating(
            dimension.FromLowerString<ReviewDimension>(),
            rating.Value,
            note);
        
        ((List<ReviewDimensionRating>)review.DimensionRatings).Add(dimensionRating);
    }
}