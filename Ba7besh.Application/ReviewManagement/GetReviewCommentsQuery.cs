using System.Data;
using Ba7besh.Application.Helpers;
using Dapper;
using Paramore.Darker;
using Paramore.Darker.QueryLogging;

namespace Ba7besh.Application.ReviewManagement;

public record GetReviewCommentsQuery(string ReviewId) : IQuery<GetReviewCommentsResult>
{
    public int PageSize { get; init; } = 20;
    public int PageNumber { get; init; } = 1;
}


public class GetReviewCommentsQueryHandler(IDbConnection db) 
    : QueryHandlerAsync<GetReviewCommentsQuery, GetReviewCommentsResult>
{
    [QueryLogging(1)]
    public override async Task<GetReviewCommentsResult> ExecuteAsync(
        GetReviewCommentsQuery query,
        CancellationToken cancellationToken = default)
    {
        await ReviewHelpers.ValidateReviewExists(db, query.ReviewId);
        
        const string sql = """
                           WITH filtered_comments AS (
                               SELECT *
                               FROM review_comments
                               WHERE review_id = @ReviewId
                               AND is_deleted = FALSE
                           ),
                           paginated_comments AS (
                               SELECT *
                               FROM filtered_comments
                               ORDER BY created_at DESC
                               OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                           )
                           SELECT 
                               pc.*,
                               (SELECT COUNT(*)::int FROM filtered_comments) as total_count
                           FROM paginated_comments pc
                           """;

        var totalCount = 0;
        var comments = await db.QueryAsync<ReviewCommentSummary, int, ReviewCommentSummary>(
            sql,
            (comment, count) =>
            {
                totalCount = count;
                return comment;
            },
            new
            {
                ReviewId = query.ReviewId,
                Offset = (query.PageNumber - 1) * query.PageSize,
                PageSize = query.PageSize
            },
            splitOn: "total_count");

        return new GetReviewCommentsResult
        {
            Comments = comments.ToList(),
            TotalCount = totalCount,
            PageSize = query.PageSize,
            PageNumber = query.PageNumber
        };
    }
}