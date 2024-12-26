using System.Data;
using System.Diagnostics;
using Dapper;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Ba7besh.Application.Tests;

public class PostgresContainerFixture : IAsyncLifetime
{
    private const string DatabaseName = "ba7besh_test";
    private const string DatabaseUserName = "test_user";
    private const string DatabasePassword = "test_password";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase(DatabaseName)
        .WithUsername(DatabaseUserName)
        .WithPassword(DatabasePassword)
        .WithImage("postgis/postgis:15-3.3")
        .Build();

    private IDbConnection? _connection;

    public IDbConnection Connection => _connection ?? throw new InvalidOperationException("Container not initialized");

    public async Task InitializeAsync()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        await _container.StartAsync();
        _connection = new NpgsqlConnection(_container.GetConnectionString());
        await RunMigrations();
    }

    public async Task DisposeAsync()
    {
        if (_connection is not null) await ((NpgsqlConnection)_connection).DisposeAsync();
        await _container.DisposeAsync();
    }

    private async Task RunMigrations()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments =
                $"./migrate.sh -h {_container.Hostname} -p {_container.GetMappedPublicPort(5432)} -d {DatabaseName} -u {DatabaseUserName} -w {DatabasePassword}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo);
        if (process == null) throw new Exception("Failed to start migrate.sh");

        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"Migration failed: {error}");
        }
    }
}