using System.Data;
using Dapper;
using Paramore.Darker;
using Paramore.Darker.QueryLogging;

namespace Ba7besh.Application.TagManagement;

public record GetTagsQuery : IQuery<IReadOnlyList<string>>;

public class GetTagsQueryHandler(IDbConnection db)
    : QueryHandlerAsync<GetTagsQuery, IReadOnlyList<string>>
{
    [QueryLogging(1)]
    public override async Task<IReadOnlyList<string>> ExecuteAsync(
        GetTagsQuery query,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                            SELECT tag
                            FROM business_tags
                            WHERE is_deleted = FALSE;
                           """;
        var dbResult = await db.QueryAsync<string>(sql, cancellationToken);
        return dbResult.ToList()
            .AsReadOnly();
    }
}