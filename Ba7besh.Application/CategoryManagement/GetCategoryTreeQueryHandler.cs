using System.Data;
using Dapper;
using Paramore.Darker;
using Paramore.Darker.QueryLogging;

namespace Ba7besh.Application.CategoryManagement;

public record GetCategoryTreeQuery : IQuery<IReadOnlyList<CategoryTreeNode>>;

public class GetCategoryTreeQueryHandler(IDbConnection db)
    : QueryHandlerAsync<GetCategoryTreeQuery, IReadOnlyList<CategoryTreeNode>>
{
    [QueryLogging(1)]
    public override async Task<IReadOnlyList<CategoryTreeNode>> ExecuteAsync(
        GetCategoryTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                           SELECT *
                           FROM categories 
                           WHERE is_deleted = FALSE;
                           """;

        var flatCategories = await db.QueryAsync<CategoryTreeNode>(sql, cancellationToken);
        var lookup = flatCategories.ToLookup(c => c.ParentId);
        return lookup[null].Select(c => new CategoryTreeNode
        {
            Id = c.Id,
            ArName = c.ArName,
            EnName = c.EnName,
            Slug = c.Slug,
            SubCategories = BuildSubCategories(c.Id, lookup)
        }).ToList();
    }

    private static List<CategoryTreeNode> BuildSubCategories(string parentId, ILookup<string?, CategoryTreeNode> lookup)
    {
        return lookup[parentId].Select(c => new CategoryTreeNode
        {
            Id = c.Id,
            ArName = c.ArName,
            EnName = c.EnName,
            Slug = c.Slug,
            SubCategories = BuildSubCategories(c.Id, lookup),
            ParentId = parentId
        }).ToList();
    }
}