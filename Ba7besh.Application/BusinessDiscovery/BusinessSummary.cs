namespace Ba7besh.Application.BusinessDiscovery;

public record BusinessSummary
{
    public required string Id { get; init; }
    public required string ArName { get; init; }
    public required string EnName { get; init; }
    public required Location Location { get; set; }
    public required string City { get; init; }
    public required string Type { get; init; }
    public List<CategoryInfo> Categories { get; init; } = [];
    public List<WorkingHours> WorkingHours { get; init; } = [];
    public List<string> Tags { get; init; } = [];
}