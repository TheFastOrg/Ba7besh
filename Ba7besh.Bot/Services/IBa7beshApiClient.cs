using Ba7besh.Application.BusinessDiscovery;
using Ba7besh.Application.ReviewManagement;

namespace Ba7besh.Bot.Services;

public interface IBa7beshApiClient
{
    Task<SearchBusinessesResult> SearchBusinessesAsync(
        SearchBusinessesQuery query,
        CancellationToken cancellationToken = default);

    Task<bool> SubmitReviewAsync(SubmitReviewCommand review, string? userFirebaseToken = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessSummaryWithStats>> GetTopRatedBusinessesAsync(
        CancellationToken cancellationToken = default);
    Task<BusinessSummary?> FindBusinessByNameAsync(string businessName, CancellationToken cancellationToken = default);

}