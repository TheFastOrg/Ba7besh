using System.Data;
using Ba7besh.Application.Exceptions;
using Dapper;

namespace Ba7besh.Application.Helpers;

public static class BusinessHelpers
{
    public static async Task ValidateBusinessExists(IDbConnection db, string businessId)
    {
        var businessExists = await db.QuerySingleOrDefaultAsync<bool>(
            "SELECT EXISTS(SELECT 1 FROM businesses WHERE id = @Id AND is_deleted = FALSE)",
            new { Id = businessId });

        if (!businessExists)
            throw new BusinessNotFoundException(businessId);
    }
}