using System.Data;
using Npgsql;

namespace Ba7besh.Application.Tests;

public static class DbConnectionExtensions
{
    public static async Task ExecuteAsync(this IDbConnection connection, string sql)
    {
        if (connection.State != ConnectionState.Open)
            await ((NpgsqlConnection)connection).OpenAsync();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        await ((NpgsqlCommand)cmd).ExecuteNonQueryAsync();
    }
}