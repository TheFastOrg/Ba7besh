using System.Data;
using System.Diagnostics;
using Ba7besh.Application.TagManagement;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Ba7besh.Application.Tests;

public class GetTagsTests : DatabaseTestBase
{
    private readonly GetTagsQueryHandler _handler;

    public GetTagsTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _handler = new GetTagsQueryHandler(Connection);
    }
    protected override async Task SeedTestData()
    {
        await Connection.ExecuteAsync(@"
            INSERT INTO businesses (id, ar_name, en_name, location, country, type, status, slug) VALUES 
            ('b1', 'مطعم 1', 'Restaurant 1', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-1'),
            ('b2', 'مطعم 2', 'Restaurant 2', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-2'),
            ('b3', 'مطعم 3', 'Restaurant 3', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-3')
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO business_tags (id, tag, business_id, created_at, is_deleted) VALUES
            ('1', 'Pizza', 'b1', CURRENT_TIMESTAMP, false),
            ('2', 'Burger', 'b1', CURRENT_TIMESTAMP, false),
            ('3', 'Healthy', 'b2', CURRENT_TIMESTAMP, false),
            ('4', 'Deleted Tag', 'b3', CURRENT_TIMESTAMP, true)
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
        await Connection.ExecuteAsync("DELETE FROM business_tags");
        var result = await _handler.ExecuteAsync(new GetTagsQuery());
        Assert.Empty(result);
    }
}