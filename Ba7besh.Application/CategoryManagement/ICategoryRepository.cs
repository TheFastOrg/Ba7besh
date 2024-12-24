namespace Ba7besh.Application.CategoryManagement;

public interface ICategoryRepository
{
    Task<IReadOnlyList<CategoryTreeNode>> GetCategoryTreeAsync(CancellationToken cancellationToken);
}