using Paramore.Darker;
using Paramore.Darker.QueryLogging;

namespace Ba7besh.Application.CategoryManagement;

public class GetCategoryTreeQueryHandler(ICategoryRepository categoryRepository)
    : QueryHandlerAsync<GetCategoryTreeQuery, IReadOnlyList<CategoryTreeNode>>
{
    
    [QueryLogging(1)]
    public override async Task<IReadOnlyList<CategoryTreeNode>> ExecuteAsync(
        GetCategoryTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        return await categoryRepository.GetCategoryTreeAsync(cancellationToken);
    }
}