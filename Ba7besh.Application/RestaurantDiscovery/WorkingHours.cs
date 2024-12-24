using System.Text.Json.Serialization;

namespace Ba7besh.Application.RestaurantDiscovery;

public record WorkingHours
{
    [JsonPropertyName("day")]
    public int DayOfWeek { get; init; }
    
    [JsonPropertyName("opening_time")]
    public string OpeningTime { get; init; } = string.Empty;
    
    [JsonPropertyName("closing_time")]
    public string ClosingTime { get; init; } = string.Empty;
}