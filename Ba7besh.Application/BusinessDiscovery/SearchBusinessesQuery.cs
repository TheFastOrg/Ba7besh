using Paramore.Darker;

namespace Ba7besh.Application.BusinessDiscovery;

public record SearchBusinessesQuery : IQuery<SearchBusinessesResult>
{
    public string? SearchTerm { get; init; }
    public string? CategoryId { get; init; }
    public string[]? Tags { get; init; }
    public int PageSize { get; init; } = 20;
    public int PageNumber { get; init; } = 1;
}