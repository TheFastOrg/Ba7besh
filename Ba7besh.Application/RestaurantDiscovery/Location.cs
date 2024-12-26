namespace Ba7besh.Application.RestaurantDiscovery;

public record Location
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}