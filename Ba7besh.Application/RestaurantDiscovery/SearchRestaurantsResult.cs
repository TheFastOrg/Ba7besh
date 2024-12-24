namespace Ba7besh.Application.RestaurantDiscovery;

public record SearchRestaurantsResult
{
    public IReadOnlyList<RestaurantSummary> Restaurants { get; init; } = Array.Empty<RestaurantSummary>();
    public int TotalCount { get; init; }
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
}