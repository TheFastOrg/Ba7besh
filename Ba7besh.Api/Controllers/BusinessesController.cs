using Asp.Versioning;
using Ba7besh.Application.BusinessDiscovery;
using Ba7besh.Application.CategoryManagement;
using Ba7besh.Application.ReviewManagement;
using Ba7besh.Application.TagManagement;
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
    [HttpGet("search")]
    public async Task<ActionResult<SearchBusinessesResult>> Search(
        [FromQuery] string? searchTerm,
        [FromQuery] string? categoryId,
        [FromQuery] string[]? tags,
        [FromQuery] int pageSize = 20,
        [FromQuery] int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchBusinessesQuery
        {
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            Tags = tags,
            PageSize = pageSize,
            PageNumber = pageNumber
        };

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
    public async Task<IActionResult> SubmitReview(
        string businessId,
        [FromBody] SubmitReviewCommand command,
        CancellationToken cancellationToken)
    {
        // TODO: Get actual user ID from auth/claims
        var userId = "123";

        command.BusinessId = businessId;
        command.UserId = userId;
        await commandProcessor.SendAsync(command, cancellationToken: cancellationToken);
        return Ok();
    }
}