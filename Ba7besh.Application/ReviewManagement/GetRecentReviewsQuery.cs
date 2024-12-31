using System.Data;
using Ba7besh.Application.Helpers;
using Dapper;
using Paramore.Darker;
using Paramore.Darker.QueryLogging;

namespace Ba7besh.Application.ReviewManagement;

public record GetRecentReviewsQuery : IQuery<IReadOnlyList<RecentReviewSummary>>
{
    public int Limit { get; init; } = 20;
}

public class GetRecentReviewsQueryHandler(IDbConnection db) 
    : QueryHandlerAsync<GetRecentReviewsQuery, IReadOnlyList<RecentReviewSummary>>
{
    [QueryLogging(1)]
    public override async Task<IReadOnlyList<RecentReviewSummary>> ExecuteAsync(
        GetRecentReviewsQuery query,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                r.id,
                r.user_id AS reviewer_name,
                r.overall_rating,
                r.content,
                r.business_id,
                r.created_at,
                b.ar_name AS business_ar_name,
                b.en_name AS business_en_name,
                b.city AS business_city,
                rr.dimension::text,
                rr.rating,
                rr.note,
                (
                    SELECT json_build_object(
                        'helpful_count', COALESCE(COUNT(*) FILTER (WHERE reaction = 'helpful'), 0),
                        'unhelpful_count', COALESCE(COUNT(*) FILTER (WHERE reaction = 'unhelpful'), 0)
                    )
                    FROM review_reactions 
                    WHERE review_id = r.id AND is_deleted = FALSE
                ) AS reactions
            FROM reviews r
            JOIN businesses b ON r.business_id = b.id AND b.is_deleted = FALSE
            LEFT JOIN review_ratings rr ON r.id = rr.review_id
            WHERE r.is_deleted = FALSE 
            AND r.status = 'approved'
            ORDER BY r.created_at DESC
            LIMIT @Limit
            """;

        var reviewDict = new Dictionary<string, RecentReviewSummary>();

        await db.QueryAsync<
            RecentReviewSummary,
            string,
            decimal,
            string,
            string,
            RecentReviewSummary>(
            sql,
            (review, dimension, rating, note, reactions) =>
            {

                if (!reviewDict.TryGetValue(review.Id, out var existingReview))
                {
                    var reactionData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(reactions);
                    review = review with
                    {
                        ReactionsSummary = new ReviewReactionsSummary
                        {
                            HelpfulCount = reactionData?["helpful_count"] ?? 0,
                            UnhelpfulCount = reactionData?["unhelpful_count"] ?? 0
                        }
                    };
                    review = review with { DimensionRatings = new List<ReviewDimensionRating>() };
                    reviewDict[review.Id] = review;
                    existingReview = review;
                }

                if (!string.IsNullOrEmpty(dimension))
                {
                    ((List<ReviewDimensionRating>)existingReview.DimensionRatings).Add(
                        new ReviewDimensionRating(
                            dimension.FromLowerString<ReviewDimension>(),
                            rating,
                            note));
                }

                return review;
            },
            new
            {
               query.Limit
            },
            splitOn: "dimension,rating,note,reactions");

        return reviewDict.Values.ToList();
    }
}