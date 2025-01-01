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
                rp.id,
                rp.review_id,
                rp.photo_url,
                rp.description,
                rp.mime_type,
                rp.size_bytes,
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
            LEFT JOIN review_photos rp ON r.id = rp.review_id AND rp.is_deleted = FALSE
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
            ReviewPhoto?,
            string,
            RecentReviewSummary>(
            sql,
            (review, dimension, rating, note, photo, reactions) =>
            {
                if (!reviewDict.TryGetValue(review.Id, out var existingReview))
                {
                    var reactionSummary = ReviewHelpers.MapReactionsSummary(reactions);
                    review = review with
                    {
                        ReactionsSummary = reactionSummary,
                        DimensionRatings = new List<ReviewDimensionRating>()
                    };
                    reviewDict[review.Id] = review;
                    existingReview = review;
                }
                ReviewHelpers.AddDimensionRating(reviewDict, review.Id, dimension, rating, note);
                ReviewHelpers.AddReviewPhoto(existingReview, photo);
                return review;
            },
            new
            {
               query.Limit
            },
            splitOn: "dimension,rating,note,id,reactions");

        return reviewDict.Values.ToList();
    }
}