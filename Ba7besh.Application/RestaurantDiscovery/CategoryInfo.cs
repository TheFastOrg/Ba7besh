namespace Ba7besh.Application.RestaurantDiscovery;

public record CategoryInfo
{
    public required string Id { get; init; }
    public required string ArName { get; init; }
    public required string EnName { get; init; }
}