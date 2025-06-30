using System.Net.Http.Json;
using Ba7besh.Application.BusinessDiscovery;
using Ba7besh.Application.ReviewManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ba7besh.Bot.Services;

public class Ba7beshApiClient : IBa7beshApiClient
{

    private readonly HttpClient _httpClient;
    private readonly ILogger<Ba7beshApiClient> _logger;
    
    public Ba7beshApiClient(HttpClient httpClient, ILogger<Ba7beshApiClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
    
        // Get API token from configuration
        var apiToken = configuration["Api:AuthToken"];
        if (!string.IsNullOrEmpty(apiToken))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Bot-Api-Key", apiToken);
        
            _logger.LogInformation("API authentication configured");
        }
        else
        {
            _logger.LogError("API authentication token is missing");
        }
    }
    
    public async Task<SearchBusinessesResult> SearchBusinessesAsync(SearchBusinessesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Log the JSON being sent
            var json = System.Text.Json.JsonSerializer.Serialize(query);
            _logger.LogInformation("Sending search query: {Json}", json);
            
            var response = await _httpClient.PostAsJsonAsync("businesses/search", query, cancellationToken);
            // Handle 404 specifically - return empty result instead of throwing
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("No businesses found for search query: {Query}", query.SearchTerm ?? "location-based search");
                return new SearchBusinessesResult(); // Return empty result
            }
            // Enhanced error logging
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("API request failed: Status={Status}, Error={Error}", 
                    response.StatusCode, 
                    string.IsNullOrEmpty(errorContent) ? "(no content)" : errorContent);
            }
            
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

    public async Task<bool> SubmitReviewAsync(SubmitReviewCommand review, string? userFirebaseToken = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"businesses/{review.BusinessId}/reviews");
    
            // FIXED: Always use Firebase token for user authentication when available
            if (!string.IsNullOrEmpty(userFirebaseToken))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userFirebaseToken);
                _logger.LogInformation("Using Firebase token for user authentication");
            }
            else
            {
                _logger.LogWarning("No Firebase token provided - review submission may fail authentication");
            }
        
            // Bot token is already set in _httpClient.DefaultRequestHeaders as X-Bot-Api-Key
            // This ensures device validation is bypassed
    
            request.Content = JsonContent.Create(new SubmitReviewCommand 
            { 
                OverallRating = review.OverallRating, 
                Content = review.Content 
            });
    
            var response = await _httpClient.SendAsync(request, cancellationToken);
        
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Submit review failed: Status={Status}, Error={Error}", 
                    response.StatusCode, errorContent);
            }
        
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