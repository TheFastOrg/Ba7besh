namespace Ba7besh.Application.RestaurantDiscovery;

public interface IRestaurantSearchService
{
    Task<SearchRestaurantsResult> SearchAsync(SearchRestaurantsQuery query, CancellationToken cancellationToken);
}