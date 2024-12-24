namespace Ba7besh.Application.CategoryManagement;

public record CategoryTreeNode
{
    public required string Id { get; init; }
    public required string ArName { get; init; }
    public required string EnName { get; init; }
    public required string Slug { get; init; }
    public IReadOnlyList<CategoryTreeNode> SubCategories { get; init; } = Array.Empty<CategoryTreeNode>();
}