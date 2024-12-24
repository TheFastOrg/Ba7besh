using Paramore.Darker;

namespace Ba7besh.Application.TagManagement;

public record GetTagsQuery : IQuery<IReadOnlyList<string>>;

public class GetTagsQueryHandler(ITagRepository tagRepository) 
    : QueryHandlerAsync<GetTagsQuery, IReadOnlyList<string>>
{
    public override async Task<IReadOnlyList<string>> ExecuteAsync(
        GetTagsQuery query, 
        CancellationToken cancellationToken = default)
    {
        return await tagRepository.GetTagsAsync(cancellationToken);
    }
}