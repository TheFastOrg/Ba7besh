using Paramore.Darker;

namespace Ba7besh.Application.CategoryManagement;

public class GetCategoryTreeQueryHandler(ICategoryRepository categoryRepository)
    : QueryHandlerAsync<GetCategoryTreeQuery, IReadOnlyList<CategoryTreeNode>>
{
    public override async Task<IReadOnlyList<CategoryTreeNode>> ExecuteAsync(
        GetCategoryTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        return await categoryRepository.GetCategoryTreeAsync(cancellationToken);
    }
}