using System.Data;
using Dapper;
using Paramore.Brighter;

namespace Ba7besh.Application.BusinessDiscovery;

public class SuggestBusinessCommandHandler(IDbConnection db) : RequestHandlerAsync<SuggestBusinessCommand>
{
    public override async Task<SuggestBusinessCommand> HandleAsync(
        SuggestBusinessCommand command,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                           INSERT INTO suggested_businesses (
                               id, user_id, ar_name, en_name, location, description
                           ) VALUES (
                               @Id, @UserId, @ArName, @EnName, ST_MakePoint(@Longitude, @Latitude), @Description
                           )
                           """;

        await db.ExecuteAsync(sql, new
        {
            Id = Guid.NewGuid().ToString("N"),
            command.UserId,
            command.ArName,
            command.EnName,
            command.Location.Longitude,
            command.Location.Latitude,
            command.Description
        });

        return command;
    }
}