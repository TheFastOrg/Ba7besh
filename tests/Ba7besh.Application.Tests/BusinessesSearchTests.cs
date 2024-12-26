using Ba7besh.Application.BusinessDiscovery;
using Moq;

namespace Ba7besh.Application.Tests;

public class BusinessesSearchTests: DatabaseTestBase
{
    private readonly SearchBusinessesQueryHandler _handler;

    public BusinessesSearchTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _handler = new SearchBusinessesQueryHandler(Connection);
    }

    protected override async Task SeedTestData()
    {
        await Connection.ExecuteAsync(@"
            INSERT INTO businesses (id, ar_name, en_name, location, country, type, status, slug, is_deleted) VALUES 
            ('b1', 'مطعم 1', 'Restaurant 1', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-1', FALSE),
            ('b2', 'مطعم 2', 'Restaurant 2', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-2', FALSE),
            ('b3', 'مطعم محذوف', 'Deleted Restaurant', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-3', TRUE)
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO categories (id, ar_name, en_name, slug, created_at, is_deleted) VALUES 
            ('cat1', 'تصنيف 1', 'Category 1', 'category-1', CURRENT_TIMESTAMP, false),
            ('cat2', 'تصنيف 2', 'Category 2', 'category-2', CURRENT_TIMESTAMP, false)
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO business_categories (id, business_id, category_id, created_at, is_deleted) VALUES 
            ('bc1', 'b1', 'cat1', CURRENT_TIMESTAMP, false),
            ('bc2', 'b2', 'cat2', CURRENT_TIMESTAMP, false)
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO business_tags (id, tag, business_id, created_at, is_deleted) VALUES 
            ('t1', 'Pizza', 'b1', CURRENT_TIMESTAMP, false),
            ('t2', 'Burger', 'b2', CURRENT_TIMESTAMP, false)
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO business_working_hours (id, day, opening_time, closing_time, business_id, created_at, is_deleted) VALUES 
            ('wh1', 1, '09:00', '22:00', 'b1', CURRENT_TIMESTAMP, false),
            ('wh2', 2, '09:00', '22:00', 'b2', CURRENT_TIMESTAMP, false)
        ");
    }

    [Fact]
    public async Task Should_Return_Businesses_With_Matching_Arabic_Name()
    {
        var result = await _handler.ExecuteAsync(new SearchBusinessesQuery { SearchTerm = "مطعم 1" });

        var business = Assert.Single(result.Businesses);
        Assert.Equal("مطعم 1", business.ArName);
    }

    [Fact]
    public async Task Should_Return_Businesses_With_Matching_English_Name()
    {
        var result = await _handler.ExecuteAsync(new SearchBusinessesQuery { SearchTerm = "Restaurant 1" });

        var business = Assert.Single(result.Businesses);
        Assert.Equal("Restaurant 1", business.EnName);
    }

    [Fact]
    public async Task Should_Return_Businesses_By_Category()
    {
        var result = await _handler.ExecuteAsync(new SearchBusinessesQuery { CategoryId = "cat1" });

        var business = Assert.Single(result.Businesses);
        var category = Assert.Single(business.Categories);
        Assert.Equal("Category 1", category.EnName);
    }

    [Fact]
    public async Task Should_Return_Businesses_By_Tags()
    {
        var result = await _handler.ExecuteAsync(new SearchBusinessesQuery { Tags = ["Pizza"] });

        var business = Assert.Single(result.Businesses);
        Assert.Contains("Pizza", business.Tags);
    }

    [Fact]
    public async Task Should_Not_Return_Deleted_Businesses()
    {
        var result = await _handler.ExecuteAsync(new SearchBusinessesQuery());

        Assert.DoesNotContain(result.Businesses, r => r.ArName == "مطعم محذوف");
    }

    [Fact]
    public async Task Should_Support_Pagination()
    {
        var result = await _handler.ExecuteAsync(new SearchBusinessesQuery { PageSize = 1, PageNumber = 2 });

        Assert.Single(result.Businesses);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(1, result.PageSize);
    }
}