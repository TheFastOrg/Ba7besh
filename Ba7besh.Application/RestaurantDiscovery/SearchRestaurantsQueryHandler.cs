using Paramore.Darker;
using Paramore.Darker.QueryLogging;

namespace Ba7besh.Application.RestaurantDiscovery;

public class SearchRestaurantsQueryHandler(IRestaurantSearchService searchService)
    : QueryHandlerAsync<SearchRestaurantsQuery, SearchRestaurantsResult>
{
    [QueryLogging(1)]
    public override async Task<SearchRestaurantsResult> ExecuteAsync(
        SearchRestaurantsQuery query,
        CancellationToken cancellationToken = default)
    {
        return await searchService.SearchAsync(query, cancellationToken);
    }
}