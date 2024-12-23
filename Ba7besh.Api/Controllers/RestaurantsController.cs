using Asp.Versioning;
using Ba7besh.Application.CategoryManagement;
using Ba7besh.Application.RestaurantDiscovery;
using Ba7besh.Application.TagManagement;
using Microsoft.AspNetCore.Mvc;
using Paramore.Darker;

namespace Ba7besh.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/restaurants")]
public class RestaurantsController(IQueryProcessor queryProcessor) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<SearchRestaurantsResult>> Search(
        [FromQuery] string? searchTerm,
        [FromQuery] string? categoryId,
        [FromQuery] string[]? tags,
        [FromQuery] int pageSize = 20,
        [FromQuery] int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchRestaurantsQuery
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
    public async Task<ActionResult<IReadOnlyList<CategoryTreeNode>>> GetTags(
        CancellationToken cancellationToken = default)
    {
        var query = new GetTagsQuery();
        var result = await queryProcessor.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }
}