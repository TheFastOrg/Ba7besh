using System.Data;
using Ba7besh.Application.Exceptions;
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
}