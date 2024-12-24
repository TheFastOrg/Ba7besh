namespace Ba7besh.Application.RestaurantDiscovery;

public record RestaurantSummary
{
    public required string Id { get; init; }
    public required string ArName { get; init; }
    public required string EnName { get; init; }
    public required string Location { get; init; }
    public required string City { get; init; }
    public required string Type { get; init; }
    public IReadOnlyList<CategoryInfo> Categories { get; init; } = Array.Empty<CategoryInfo>();
    public IReadOnlyList<WorkingHours> WorkingHours { get; init; } = Array.Empty<WorkingHours>();
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}