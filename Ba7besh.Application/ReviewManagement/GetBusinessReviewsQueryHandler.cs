using System.Data;
using Ba7besh.Application.Helpers;
using Dapper;
using Paramore.Darker;
using Paramore.Darker.QueryLogging;

namespace Ba7besh.Application.ReviewManagement;

public class GetBusinessReviewsQueryHandler(IDbConnection db)
    : QueryHandlerAsync<GetBusinessReviewsQuery, GetBusinessReviewsResult>
{
    [QueryLogging(1)]
    public override async Task<GetBusinessReviewsResult> ExecuteAsync(GetBusinessReviewsQuery query,
        CancellationToken cancellationToken = new())
    {
        await BusinessHelpers.ValidateBusinessExists(db, query.BusinessId);
        const string sql = """
                           WITH filtered_reviews AS (SELECT r.id,
                                                            r.user_id                  AS reviewer_name,
                                                            r.overall_rating,
                                                            r.content,
                                                            (SELECT json_build_object(
                                                                            'helpful_count',
                                                                            COALESCE(COUNT(*) FILTER (WHERE reaction = 'helpful'), 0),
                                                                            'unhelpful_count',
                                                                            COALESCE(COUNT(*) FILTER (WHERE reaction = 'unhelpful'), 0)
                                                                    )
                                                             FROM review_reactions
                                                             WHERE review_id = r.id
                                                               AND is_deleted = FALSE) AS reactions
                                                     FROM reviews AS r
                                                     WHERE r.is_deleted = FALSE
                                                       AND r.status = 'approved'
                                                       AND r.business_id = @BusinessId
                                                     ORDER BY r.created_at DESC),
                                paginated_reviews AS (SELECT *
                                                      FROM filtered_reviews
                                                      OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY)
                           SELECT r.*,
                                  rr.dimension::text,
                                  rr.rating,
                                  rr.note,
                                  rp.id,
                                  rp.review_id,
                                  rp.photo_url,
                                  rp.description,
                                  rp.mime_type,
                                  rp.size_bytes,
                                  (SELECT COUNT(*)::int FROM filtered_reviews) as total_count
                           FROM paginated_reviews AS r
                               LEFT JOIN review_ratings rr ON r.id = rr.review_id
                               LEFT JOIN review_photos rp ON r.id = rp.review_id AND rp.is_deleted = FALSE
                           """;
        var totalCount = 0;
        var reviewDict = new Dictionary<string, ReviewSummary>();
        await db.QueryAsync<
            ReviewSummary,
            string?, 
            string,
            decimal?,
            string?,
            ReviewPhoto?,
            int, 
            ReviewSummary>(sql, (review, reactions, dimension, rating, note, photo, total) =>
            {
                totalCount = total;
                if (!reviewDict.TryGetValue(review.Id, out var existingReview))
                {
                    var reactionSummary = ReviewHelpers.MapReactionsSummary(reactions);

                    review = review with
                    {
                        ReactionsSummary = reactionSummary,
                        DimensionRatings = new List<ReviewDimensionRating>(),
                        Photos = new List<ReviewPhoto>()
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
                BusinessId = query.BusinessId,
                Offset = (query.PageNumber - 1) * query.PageSize,
                PageSize = query.PageSize
            },
            splitOn: "reactions,dimension,rating,note,id,total_count");
        return new GetBusinessReviewsResult
        {
            Reviews = reviewDict.Values.ToList(),
            TotalCount = totalCount,
            PageSize = query.PageSize,
            PageNumber = query.PageNumber
        };
    }
}