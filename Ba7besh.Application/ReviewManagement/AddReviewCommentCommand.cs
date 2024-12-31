using System.Data;
using Ba7besh.Application.Helpers;
using Dapper;
using Paramore.Brighter;

namespace Ba7besh.Application.ReviewManagement;

public class AddReviewCommentCommand(string reviewId, string userId, string content) : Command(Guid.NewGuid())
{
    public string ReviewId { get; } = reviewId;
    public string UserId { get; } = userId;
    public string Content { get; } = content;
}


public class AddReviewCommentCommandHandler(IDbConnection db) : RequestHandlerAsync<AddReviewCommentCommand>
{
    public override async Task<AddReviewCommentCommand> HandleAsync(
        AddReviewCommentCommand command,
        CancellationToken cancellationToken = default)
    {
        await ReviewHelpers.ValidateReviewExists(db, command.ReviewId);

        var commentId = Guid.NewGuid().ToString("N");
        await db.ExecuteAsync("""
                              INSERT INTO review_comments (id, review_id, user_id, content, created_at)
                              VALUES (@CommentId, @ReviewId, @UserId, @Content, @CreatedAt)
                              """,
            new
            {
                CommentId = commentId,
                command.ReviewId,
                command.UserId,
                command.Content,
                CreatedAt = DateTime.UtcNow
            });

        return command;
    }
}