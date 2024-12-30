using Paramore.Darker;

namespace Ba7besh.Application.BusinessDiscovery;

public record GetTopRatedBusinessesQuery : IQuery<IReadOnlyList<BusinessSummaryWithStats>>
{
    public double MinimumRating { get; init; } = 4;
    public int Limit { get; init; } = 10;
}