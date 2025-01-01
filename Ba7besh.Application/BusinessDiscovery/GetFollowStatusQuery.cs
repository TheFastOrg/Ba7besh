using System.Data;
using Dapper;
using Paramore.Darker;

namespace Ba7besh.Application.BusinessDiscovery;

public record GetFollowStatusQuery(string BusinessId, string UserId) : IQuery<bool>;

public class GetFollowStatusQueryHandler(IDbConnection db) : QueryHandlerAsync<GetFollowStatusQuery, bool>
{
    public override async Task<bool> ExecuteAsync(
        GetFollowStatusQuery query,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                           SELECT EXISTS (
                               SELECT 1 FROM business_followers
                               WHERE business_id = @BusinessId
                               AND user_id = @UserId
                               AND is_following = TRUE
                           )
                           """;

        return await db.QuerySingleAsync<bool>(sql, new
        {
            query.BusinessId,
            query.UserId
        });
    }
}