using Ba7besh.Application.BusinessDiscovery;

namespace Ba7besh.Application.Tests;

public class GetTopRatedBusinessesTests : DatabaseTestBase
{
    private readonly GetTopRatedBusinessesQueryHandler _topRatedHandler;

    public GetTopRatedBusinessesTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _topRatedHandler = new GetTopRatedBusinessesQueryHandler(Connection);
    }

    protected override async Task SeedTestData()
    {
        await Connection.ExecuteAsync($@"
            INSERT INTO businesses (id, ar_name, en_name, location, country, type, status, slug, is_deleted) 
            VALUES 
            ('b1', 'مطعم 1', 'Restaurant 1', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-1', FALSE),
            ('b2', 'مطعم 2', 'Restaurant 2', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-2', FALSE),
            ('b3', 'مطعم 3', 'Restaurant 3', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-3', FALSE),
            ('b4', 'مطعم محذوف', 'Deleted Restaurant', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-4', TRUE)
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO reviews (id, business_id, user_id, overall_rating, content, status, created_at, is_deleted) 
            VALUES 
            ('r1', 'b1', 'u1', 4.5, 'Great!', 'approved', CURRENT_TIMESTAMP, FALSE),
            ('r2', 'b1', 'u2', 5.0, 'Amazing!', 'approved', CURRENT_TIMESTAMP, FALSE),
            ('r3', 'b2', 'u1', 3.5, 'Good', 'approved', CURRENT_TIMESTAMP, FALSE),
            ('r4', 'b3', 'u2', 4.8, 'Excellent!', 'approved', CURRENT_TIMESTAMP, FALSE)
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO categories (id, ar_name, en_name, slug, created_at, is_deleted) VALUES 
            ('cat1', 'تصنيف 1', 'Category 1', 'category-1', CURRENT_TIMESTAMP, false)
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO business_categories (id, business_id, category_id, created_at, is_deleted) VALUES 
            ('bc1', 'b1', 'cat1', CURRENT_TIMESTAMP, false)
        ");
    }

    [Fact]
    public async Task Should_Return_Top_Rated_Businesses()
    {
        var result = await _topRatedHandler.ExecuteAsync(new GetTopRatedBusinessesQuery()
        {
            MinimumRating = 4.7
        });

        Assert.Equal(2, result.Count);
        var topBusiness = result.First();
        Assert.Equal("مطعم 3", topBusiness.ArName);
        Assert.Equal(4.8m, topBusiness.AverageRating);
        Assert.Equal(1, topBusiness.ReviewCount);
    }
}