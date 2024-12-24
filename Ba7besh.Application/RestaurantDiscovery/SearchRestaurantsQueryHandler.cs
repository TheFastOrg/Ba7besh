using Paramore.Darker;

namespace Ba7besh.Application.RestaurantDiscovery;

public class SearchRestaurantsQueryHandler : QueryHandlerAsync<SearchRestaurantsQuery, SearchRestaurantsResult>
{
    private readonly IRestaurantSearchService _searchService;

    public SearchRestaurantsQueryHandler(IRestaurantSearchService searchService)
    {
        _searchService = searchService;
    }

    public override async Task<SearchRestaurantsResult> ExecuteAsync(
        SearchRestaurantsQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _searchService.SearchAsync(query, cancellationToken);
    }
}