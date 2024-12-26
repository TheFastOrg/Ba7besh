using System.Data;
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
        const string sql = """
                           WITH filtered_reviews AS (SELECT r.id,
                                                            r.user_id AS reviewer_name,
                                                            r.overall_rating,
                                                            r.content
                                                     FROM reviews AS r
                                                     WHERE r.is_deleted = FALSE
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
        var reviews = await db.QueryAsync<ReviewSummary, int, ReviewSummary>(sql, (review, count) =>
            {
                totalCount = count;
                return review;
            },
            new
            {
                BusinessId = query.BusinessId,
                Offset = (query.PageNumber - 1) * query.PageSize,
                PageSize = query.PageSize
            },
            splitOn: "total_count");
        return new GetBusinessReviewsResult
        {
            Reviews = reviews.ToList(),
            TotalCount = totalCount,
            PageSize = query.PageSize,
            PageNumber = query.PageNumber
        };
    }
}