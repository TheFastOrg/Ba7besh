using System.Net.Http.Json;
using Ba7besh.Application.BusinessDiscovery;
using Ba7besh.Application.ReviewManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ba7besh.Bot.Services;

public class Ba7beshApiClient : IBa7beshApiClient
{
    private readonly string _apiToken;
    private readonly HttpClient _httpClient;
    private readonly ILogger<Ba7beshApiClient> _logger;
    
    // Add a constructor to get the API token from configuration
    public Ba7beshApiClient(HttpClient httpClient, ILogger<Ba7beshApiClient> logger, IConfiguration configuration)
    {
        this._httpClient = httpClient;
        this._logger = logger;
        
        // Get API token from configuration (this will be set in the docker container)
        _apiToken = configuration["Api:AuthToken"] ?? string.Empty;
        
        // Add the token to the default request headers
        if (!string.IsNullOrEmpty(_apiToken))
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiToken);
        }
    }
    public async Task<SearchBusinessesResult> SearchBusinessesAsync(SearchBusinessesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("businesses/search", query, cancellationToken);
            response.EnsureSuccessStatusCode();
        
            return await response.Content.ReadFromJsonAsync<SearchBusinessesResult>(cancellationToken) 
                   ?? new SearchBusinessesResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching businesses with query: {Query}", 
                query.SearchTerm ?? "location-based search");
            throw;
        }
    }

    public async Task<bool> SubmitReviewAsync(SubmitReviewCommand review, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"businesses/{review.BusinessId}/reviews", review, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting review for business: {BusinessId}", review.BusinessId);
            throw;
        }
    }

    public async Task<IReadOnlyList<BusinessSummaryWithStats>> GetTopRatedBusinessesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("businesses/top-rated?minimumRating=4&limit=5", cancellationToken);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<List<BusinessSummaryWithStats>>(cancellationToken) 
                   ?? new List<BusinessSummaryWithStats>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top rated businesses");
            throw;
        }
    }
    public async Task<BusinessSummary?> FindBusinessByNameAsync(string businessName, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new SearchBusinessesQuery { SearchTerm = businessName };
            var result = await SearchBusinessesAsync(query, cancellationToken);
        
            // Try to find an exact or close match
            return result.Businesses.FirstOrDefault(b => 
                       b.ArName.Equals(businessName, StringComparison.OrdinalIgnoreCase) || 
                       b.EnName.Equals(businessName, StringComparison.OrdinalIgnoreCase)) ??
                   result.Businesses.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding business by name: {BusinessName}", businessName);
            return null;
        }
    }
}