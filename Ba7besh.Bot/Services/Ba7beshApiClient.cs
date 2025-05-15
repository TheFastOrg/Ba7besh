using System.Net.Http.Json;
using Ba7besh.Application.BusinessDiscovery;
using Ba7besh.Application.ReviewManagement;
using Microsoft.Extensions.Logging;

namespace Ba7besh.Bot.Services;

public class Ba7beshApiClient(HttpClient httpClient, ILogger<Ba7beshApiClient> logger) : IBa7beshApiClient
{
    public async Task<SearchBusinessesResult> SearchBusinessesAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new SearchBusinessesQuery { SearchTerm = searchTerm };
            var response = await httpClient.PostAsJsonAsync("businesses/search", query, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<SearchBusinessesResult>(cancellationToken) 
                   ?? new SearchBusinessesResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching businesses with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<bool> SubmitReviewAsync(SubmitReviewCommand review, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"businesses/{review.BusinessId}/reviews", review, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting review for business: {BusinessId}", review.BusinessId);
            throw;
        }
    }

    public async Task<IReadOnlyList<BusinessSummaryWithStats>> GetTopRatedBusinessesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync("businesses/top-rated?minimumRating=4&limit=5", cancellationToken);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<List<BusinessSummaryWithStats>>(cancellationToken) 
                   ?? new List<BusinessSummaryWithStats>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting top rated businesses");
            throw;
        }
    }
}