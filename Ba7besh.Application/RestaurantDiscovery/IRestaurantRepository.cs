namespace Ba7besh.Application.RestaurantDiscovery;

public interface IRestaurantRepository
{
    Task<(IReadOnlyList<RestaurantSummary> Restaurants, int TotalCount)> SearchAsync(
        SearchRestaurantsQuery query,
        CancellationToken cancellationToken);
}