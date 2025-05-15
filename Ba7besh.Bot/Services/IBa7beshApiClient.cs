using Ba7besh.Application.BusinessDiscovery;
using Ba7besh.Application.ReviewManagement;

namespace Ba7besh.Bot.Services;

public interface IBa7beshApiClient
{
    Task<SearchBusinessesResult> SearchBusinessesAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> SubmitReviewAsync(SubmitReviewCommand review, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BusinessSummaryWithStats>> GetTopRatedBusinessesAsync(CancellationToken cancellationToken = default);
}