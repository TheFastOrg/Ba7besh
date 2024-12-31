using Asp.Versioning;
using Ba7besh.Api.Helpers;
using Ba7besh.Application.ReviewManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paramore.Brighter;
using Paramore.Darker;

namespace Ba7besh.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reviews")]
public class ReviewsController(
    IQueryProcessor queryProcessor,
    IAmACommandProcessor commandProcessor)
    : ControllerBase
{
    [HttpPost("{reviewId}/reactions")]
    [Authorize]
    public async Task<IActionResult> ReactToReview(
        string reviewId,
        [FromBody] ReactToReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetAuthenticatedUser()?.UserId
                     ?? throw new InvalidOperationException();

        var command = new ReactToReviewCommand(reviewId, userId, request.Reaction);

        await commandProcessor.SendAsync(command, cancellationToken: cancellationToken);
        return Ok();
    }

    [HttpGet("{reviewId}/reactions")]
    public async Task<ActionResult<ReviewReactionsSummary>> GetReviewReactions(
        string reviewId,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetAuthenticatedUser()?.UserId;
        var query = new GetReviewReactionsSummaryQuery(reviewId, userId);
        var result = await queryProcessor.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }
    
    [HttpPost("{reviewId}/comments")]
    [Authorize]
    public async Task<IActionResult> AddComment(
        string reviewId,
        [FromBody] AddReviewCommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetAuthenticatedUser()?.UserId ?? throw new InvalidOperationException();
        var command = new AddReviewCommentCommand(reviewId, userId, request.Content);
        await commandProcessor.SendAsync(command, cancellationToken: cancellationToken);
        return Ok();
    }

    [HttpGet("{reviewId}/comments")]
    public async Task<ActionResult<GetReviewCommentsResult>> GetComments(
        string reviewId,
        [FromQuery] int pageSize = 20,
        [FromQuery] int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReviewCommentsQuery(reviewId)
        {
            PageSize = pageSize,
            PageNumber = pageNumber
        };
        var result = await queryProcessor.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }
    
    [HttpGet("{reviewId}/photos")]
    public async Task<ActionResult<IReadOnlyList<ReviewPhoto>>> GetReviewPhotos(
        string reviewId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReviewPhotosQuery(reviewId);
        var result = await queryProcessor.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }
    
    [HttpGet("recent")]
    public async Task<ActionResult<IReadOnlyList<RecentReviewSummary>>> GetRecentReviews(
        [FromQuery] GetRecentReviewsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await queryProcessor.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }
}

public record ReactToReviewRequest(ReviewReaction Reaction);
public record AddReviewCommentRequest(string Content);
