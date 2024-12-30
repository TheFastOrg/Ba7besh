using System.Data;
using Ba7besh.Application.Helpers;
using Dapper;
using Paramore.Darker;

namespace Ba7besh.Application.ReviewManagement;

public record GetReviewReactionsSummaryQuery(string ReviewId, string? UserId = null) : IQuery<ReviewReactionsSummary>;

public class GetReviewReactionsSummaryQueryHandler(IDbConnection db)
    : QueryHandlerAsync<GetReviewReactionsSummaryQuery, ReviewReactionsSummary>
{
    public override async Task<ReviewReactionsSummary> ExecuteAsync(
        GetReviewReactionsSummaryQuery query, 
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                          SELECT
                              COUNT(*) FILTER (WHERE reaction = 'helpful') AS helpful_count,
                              COUNT(*) FILTER (WHERE reaction = 'unhelpful') AS unhelpful_count,
                              (
                                  SELECT reaction::text
                                  FROM review_reactions
                                  WHERE review_id = @ReviewId
                                  AND user_id = @UserId
                                  AND is_deleted = FALSE
                              ) AS user_reaction
                          FROM review_reactions
                          WHERE review_id = @ReviewId AND is_deleted = FALSE
                          """;

        var result = await db.QuerySingleOrDefaultAsync<(int helpfulCount, int unhelpfulCount, string? reaction)>(
            sql,
            new { query.ReviewId, query.UserId });

        return new ReviewReactionsSummary
        {
            HelpfulCount = result.helpfulCount,
            UnhelpfulCount = result.unhelpfulCount,
            UserReaction = result.reaction?.FromLowerString<ReviewReaction>()
        };
    }
}