using System.Data;
using Ba7besh.Application.Helpers;
using Dapper;
using Paramore.Brighter;

namespace Ba7besh.Application.BusinessDiscovery;

public class UnfollowBusinessCommand(string businessId, string userId) : Command(Guid.NewGuid())
{
    public string BusinessId { get; } = businessId;
    public string UserId { get; } = userId;
}

public class UnfollowBusinessCommandHandler(IDbConnection db) : RequestHandlerAsync<UnfollowBusinessCommand>
{
    public override async Task<UnfollowBusinessCommand> HandleAsync(
        UnfollowBusinessCommand command,
        CancellationToken cancellationToken = default)
    {
        await BusinessHelpers.ValidateBusinessExists(db, command.BusinessId);

        const string sql = """
                           UPDATE business_followers
                           SET is_following = false
                           WHERE business_id = @BusinessId AND user_id = @UserId
                           """;

        await db.ExecuteAsync(sql, new
        {
            command.BusinessId,
            command.UserId
        });

        return command;
    }
}