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
                                                            rr.dimension::text,
                                                            rr.rating,
                                                            rr.note,
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
                                                            LEFT JOIN review_ratings rr ON r.id = rr.review_id
                                                     WHERE r.is_deleted = FALSE
                                                       AND r.status = 'approved'
                                                       AND r.business_id = @BusinessId
                                                     ORDER BY created_at DESC),
                                paginated_reviews AS (SELECT *
                                                      FROM filtered_reviews
                                                      OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY)
                           SELECT *,
                                  (SELECT COUNT(*)::int FROM filtered_reviews) as total_count
                           FROM paginated_reviews
                           """;
        var totalCount = 0;
        var reviewDict = new Dictionary<string, ReviewSummary>();
        await db.QueryAsync<
            ReviewSummary,
            string,
            decimal?,
            string?,
            string?, 
            int, 
            ReviewSummary>(sql, (review, dimension, rating, note, reactions, total) =>
            {
                totalCount = total;
                if (!reviewDict.ContainsKey(review.Id))
                {
                    var reactionSummary = ReviewHelpers.MapReactionsSummary(reactions);

                    review = review with
                    {
                        ReactionsSummary = reactionSummary,
                        DimensionRatings = new List<ReviewDimensionRating>()
                    };
                    reviewDict[review.Id] = review;
                }

                ReviewHelpers.AddDimensionRating(reviewDict, review.Id, dimension, rating, note);

                return review;
            },
            new
            {
                BusinessId = query.BusinessId,
                Offset = (query.PageNumber - 1) * query.PageSize,
                PageSize = query.PageSize
            },
            splitOn: "dimension,rating,note,reactions,total_count");
        return new GetBusinessReviewsResult
        {
            Reviews = reviewDict.Values.ToList(),
            TotalCount = totalCount,
            PageSize = query.PageSize,
            PageNumber = query.PageNumber
        };
    }
}