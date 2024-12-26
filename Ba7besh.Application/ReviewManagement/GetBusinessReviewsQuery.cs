using Paramore.Darker;

namespace Ba7besh.Application.ReviewManagement;

public record GetBusinessReviewsQuery(string BusinessId) : IQuery<GetBusinessReviewsResult>
{
    public int PageSize { get; init; } = 20;
    public int PageNumber { get; init; } = 1;
}
