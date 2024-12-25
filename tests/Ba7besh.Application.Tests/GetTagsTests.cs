using System.Data;
using System.Diagnostics;
using Ba7besh.Application.TagManagement;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Ba7besh.Application.Tests;

public class GetTagsTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private GetTagsQueryHandler _handler;
    private IDbConnection _connection;

    public GetTagsTests()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("ba7besh_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithImage("postgis/postgis:15-3.3")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        _connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await RunMigrations();
        await SeedTestData();
        _handler = new GetTagsQueryHandler(_connection);
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    private async Task RunMigrations()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments =
                $"./migrate.sh -h {_dbContainer.Hostname} -p {_dbContainer.GetMappedPublicPort(5432)} -d ba7besh_test -u test_user -w test_password",
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

    private async Task SeedTestData()
    {
        await _connection.ExecuteAsync(@"
        INSERT INTO businesses (id, ar_name, en_name, location, country, type, status, slug) VALUES 
        ('b1', 'مطعم 1', 'Restaurant 1', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-1'),
        ('b2', 'مطعم 2', 'Restaurant 2', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-2'),
        ('b3', 'مطعم 3', 'Restaurant 3', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-3')
    ");

        await _connection.ExecuteAsync(@"
        INSERT INTO business_tags (id, tag, business_id, created_at, is_deleted) VALUES
        ('1', 'Pizza', 'b1', CURRENT_TIMESTAMP, false),
        ('2', 'Burger', 'b1', CURRENT_TIMESTAMP, false),
        ('3', 'Healthy', 'b2', CURRENT_TIMESTAMP, false),
        ('4', 'Deleted Tag', 'b3', CURRENT_TIMESTAMP, true);
    ");
    }

    [Fact]
    public async Task Should_Return_All_Active_Tags()
    {
        var result = await _handler.ExecuteAsync(new GetTagsQuery());

        Assert.Equal(3, result.Count);
        Assert.Contains("Pizza", result);
        Assert.Contains("Burger", result);
        Assert.Contains("Healthy", result);
        Assert.DoesNotContain("Deleted Tag", result);
    }

    [Fact]
    public async Task Should_Handle_Empty_Tags_List()
    {
        await _connection.ExecuteAsync("DELETE FROM business_tags");
        var result = await _handler.ExecuteAsync(new GetTagsQuery());
        Assert.Empty(result);
    }
}