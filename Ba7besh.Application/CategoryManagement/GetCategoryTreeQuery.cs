using Paramore.Darker;

namespace Ba7besh.Application.CategoryManagement;

public record GetCategoryTreeQuery : IQuery<IReadOnlyList<CategoryTreeNode>>
{
}