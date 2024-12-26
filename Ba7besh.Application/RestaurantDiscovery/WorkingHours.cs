namespace Ba7besh.Application.RestaurantDiscovery;

public record WorkingHours
{
    public int Day { get; init; }
    
    public string OpeningTime { get; init; } = string.Empty;
    
    public string ClosingTime { get; init; } = string.Empty;
}