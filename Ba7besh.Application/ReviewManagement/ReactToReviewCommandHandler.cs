using System.Data;
using Ba7besh.Application.Helpers;
using Dapper;
using Paramore.Brighter;

namespace Ba7besh.Application.ReviewManagement;

public class ReactToReviewCommandHandler(IDbConnection db) : RequestHandlerAsync<ReactToReviewCommand>
{
    public override async Task<ReactToReviewCommand> HandleAsync(
        ReactToReviewCommand command, 
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                          WITH existing_reaction AS (
                              SELECT id, reaction::text
                              FROM review_reactions
                              WHERE review_id = @ReviewId 
                              AND user_id = @UserId
                              AND is_deleted = FALSE
                          )
                          INSERT INTO review_reactions (id, review_id, user_id, reaction)
                          SELECT @Id, @ReviewId, @UserId, @Reaction::review_reaction
                          WHERE NOT EXISTS (SELECT 1 FROM existing_reaction)
                          RETURNING id
                          """;

        var reactionId = await db.QuerySingleOrDefaultAsync<string?>(sql, new
        {
            Id = Guid.NewGuid().ToString("N"),
            command.ReviewId,
            command.UserId,
            Reaction = command.Reaction.ToLowerString()
        });

        if (reactionId == null)
        {
            await db.ExecuteAsync("""
                                  UPDATE review_reactions
                                  SET reaction = @Reaction::review_reaction,
                                      updated_at = CURRENT_TIMESTAMP
                                  WHERE review_id = @ReviewId
                                  AND user_id = @UserId
                                  AND is_deleted = FALSE
                                  AND reaction != @Reaction::review_reaction
                                  """,
                new
                {
                    command.ReviewId,
                    command.UserId,
                    Reaction = command.Reaction.ToString().ToLower()
                });
        }

        return command;
    }
}