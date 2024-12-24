using Paramore.Darker;
using Paramore.Darker.QueryLogging;

namespace Ba7besh.Application.TagManagement;

public record GetTagsQuery : IQuery<IReadOnlyList<string>>;

public class GetTagsQueryHandler(ITagRepository tagRepository) 
    : QueryHandlerAsync<GetTagsQuery, IReadOnlyList<string>>
{
    [QueryLogging(1)]
    public override async Task<IReadOnlyList<string>> ExecuteAsync(
        GetTagsQuery query, 
        CancellationToken cancellationToken = default)
    {
        return await tagRepository.GetTagsAsync(cancellationToken);
    }
}