namespace Ba7besh.Application.TagManagement;

public interface ITagRepository
{
    Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken cancellationToken);
}