using System.Data;
using Ba7besh.Application.Helpers;
using Dapper;
using Paramore.Brighter;

namespace Ba7besh.Application.BusinessDiscovery;

public class FollowBusinessCommand(string businessId, string userId) : Command(Guid.NewGuid())
{
    public string BusinessId { get; } = businessId;
    public string UserId { get; } = userId;
}

public class FollowBusinessCommandHandler(IDbConnection db) : RequestHandlerAsync<FollowBusinessCommand>
{
    public override async Task<FollowBusinessCommand> HandleAsync(
        FollowBusinessCommand command,
        CancellationToken cancellationToken = default)
    {
        await BusinessHelpers.ValidateBusinessExists(db, command.BusinessId);

        const string sql = """
                           INSERT INTO business_followers (id, business_id, user_id)
                           VALUES (@Id, @BusinessId, @UserId)
                           ON CONFLICT (business_id, user_id) 
                           DO UPDATE SET is_following = true
                           """;

        await db.ExecuteAsync(sql, new
        {
            Id = Guid.NewGuid().ToString("N"),
            command.BusinessId,
            command.UserId
        });

        return command;
    }
}