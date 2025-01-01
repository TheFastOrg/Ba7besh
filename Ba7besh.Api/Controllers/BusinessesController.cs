using Asp.Versioning;
using Ba7besh.Api.Helpers;
using Ba7besh.Api.Models;
using Ba7besh.Application.BusinessDiscovery;
using Ba7besh.Application.CategoryManagement;
using Ba7besh.Application.ReviewManagement;
using Ba7besh.Application.TagManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paramore.Brighter;
using Paramore.Darker;

namespace Ba7besh.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/businesses")]
public class BusinessesController(IQueryProcessor queryProcessor, IAmACommandProcessor commandProcessor)
    : ControllerBase
{
    [HttpPost("search")]
    public async Task<ActionResult<SearchBusinessesResult>> Search(
        [FromBody] SearchBusinessesQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await queryProcessor.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<CategoryTreeNode>>> GetCategories(
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoryTreeQuery();
        var result = await queryProcessor.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("tags")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTags(
        CancellationToken cancellationToken = default)
    {
        var query = new GetTagsQuery();
        var result = await queryProcessor.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }
    
    [HttpGet("top-rated")]
    public async Task<ActionResult<IReadOnlyList<BusinessSummaryWithStats>>> GetTopRatedBusinesses(
        [FromQuery] int minimumRating = 4,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTopRatedBusinessesQuery 
        { 
            MinimumRating = minimumRating,
            Limit = limit
        };
        var result = await queryProcessor.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }
    
    [HttpGet("recommendations")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<BusinessSummaryWithStats>>> GetRecommendations(
        [FromQuery] Location? location = null,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetAuthenticatedUser()?.UserId 
                     ?? throw new InvalidOperationException();
        
        var query = new GetPersonalizedRecommendationsQuery(userId, location, limit);
        var result = await queryProcessor.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{businessId}/reviews")]
    [Authorize]
    public async Task<IActionResult> SubmitReview(
        string businessId,
        [FromForm] SubmitReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetAuthenticatedUser()?.UserId ?? throw new InvalidOperationException();
        var command = new SubmitReviewCommand
        {
            BusinessId = businessId,
            UserId = userId,
            OverallRating = request.OverallRating,
            Content = request.Content,
            DimensionRatings = request.DimensionRatings ?? [],
            Photos = request.Photos?.Select(p => new ReviewPhotoDto(p.FileBase64, p.FileName, p.Description)).ToList() ?? [], 
        };
        await commandProcessor.SendAsync(command, cancellationToken: cancellationToken);
        return Ok();
    }

    [HttpGet("{businessId}/reviews")]
    public async Task<ActionResult<GetBusinessReviewsResult>> GetBusinessReviews(
        string businessId,
        [FromQuery] int pageSize = 20,
        [FromQuery] int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBusinessReviewsQuery(businessId)
        {
            PageSize = pageSize,
            PageNumber = pageNumber
        };
        var result = await queryProcessor.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }
}

public class SubmitReviewRequest
{
    public decimal OverallRating { get; init; }
    public string? Content { get; init; }
    public ReviewDimensionRating[]? DimensionRatings { get; init; }
    public ReviewPhotoRequest[]? Photos { get; init; }
}

public class ReviewPhotoRequest
{
    public string FileBase64 { get; init; } = string.Empty;
    public string? FileName { get; init; }
    public string? Description { get; init; }
}