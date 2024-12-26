namespace Ba7besh.Application.BusinessDiscovery;

public record SearchBusinessesResult
{
    public IReadOnlyList<BusinessSummary> Businesses { get; init; } = Array.Empty<BusinessSummary>();
    public int TotalCount { get; init; }
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
}