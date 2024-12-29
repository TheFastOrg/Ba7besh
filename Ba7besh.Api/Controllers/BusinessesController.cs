using Asp.Versioning;
using Ba7besh.Api.Helpers;
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

    [HttpPost("{businessId}/reviews")]
    [Authorize]
    public async Task<IActionResult> SubmitReview(
        string businessId,
        [FromBody] SubmitReviewCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetAuthenticatedUser()?.UserId ?? throw new InvalidOperationException();

        command.BusinessId = businessId;
        command.UserId = userId;
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