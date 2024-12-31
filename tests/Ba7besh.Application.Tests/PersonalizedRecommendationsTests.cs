using Ba7besh.Application.BusinessDiscovery;

namespace Ba7besh.Application.Tests;

public class PersonalizedRecommendationsTests : DatabaseTestBase
{
    private readonly GetPersonalizedRecommendationsQueryHandler _handler;
    private const string TestUserId = "test_user";
    private const double DamascusLatitude = 33.5138;
    private const double DamascusLongitude = 36.2765;

    public PersonalizedRecommendationsTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _handler = new GetPersonalizedRecommendationsQueryHandler(Connection);
    }

    protected override async Task SeedTestData()
    {
        await Connection.ExecuteAsync($@"
            INSERT INTO businesses (id, ar_name, en_name, location, country, type, status, slug, is_deleted) VALUES 
            ('b1', 'مطعم 1', 'Restaurant 1', ST_MakePoint({DamascusLongitude}, {DamascusLatitude}), 'SY', 'restaurant', 'active', 'restaurant-1', FALSE),
            ('b2', 'مطعم 2', 'Restaurant 2', ST_MakePoint({DamascusLongitude + 0.018}, {DamascusLatitude}), 'SY', 'restaurant', 'active', 'restaurant-2', FALSE),
            ('b3', 'مطعم 3', 'Restaurant 3', ST_MakePoint({DamascusLongitude}, {DamascusLatitude}), 'SY', 'restaurant', 'active', 'restaurant-3', FALSE),
            ('b4', 'مطعم محذوف', 'Deleted Restaurant', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-4', TRUE)
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO categories (id, ar_name, en_name, slug, created_at, is_deleted) VALUES 
            ('cat1', 'شرقي', 'Eastern', 'eastern', CURRENT_TIMESTAMP, false),
            ('cat2', 'إيطالي', 'Italian', 'italian', CURRENT_TIMESTAMP, false)
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO business_categories (id, business_id, category_id, created_at, is_deleted) VALUES 
            ('bc1', 'b1', 'cat1', CURRENT_TIMESTAMP, false),
            ('bc2', 'b2', 'cat1', CURRENT_TIMESTAMP, false),
            ('bc3', 'b3', 'cat2', CURRENT_TIMESTAMP, false)
        ");

        await Connection.ExecuteAsync($@"
            INSERT INTO reviews (id, business_id, user_id, overall_rating, content, status, created_at, is_deleted) VALUES 
            ('r1', 'b1', '{TestUserId}', 4.5, 'Great eastern food!', 'approved', CURRENT_TIMESTAMP, FALSE),
            ('r2', 'b1', 'other_user', 4.0, 'Nice!', 'approved', CURRENT_TIMESTAMP, FALSE),
            ('r3', 'b2', 'other_user', 4.8, 'Amazing!', 'approved', CURRENT_TIMESTAMP, FALSE),
            ('r5', 'b2', 'other_user', 4.8, 'Amazing!', 'approved', CURRENT_TIMESTAMP, FALSE),
            ('r6', 'b2', 'other_user', 4.8, 'Amazing!', 'approved', CURRENT_TIMESTAMP, FALSE),
            ('r7', 'b2', 'other_user', 4.8, 'Amazing!', 'approved', CURRENT_TIMESTAMP, FALSE),            
            ('r4', 'b3', 'other_user', 4.2, 'Good!', 'approved', CURRENT_TIMESTAMP, FALSE)");
    }

    [Fact]
    public async Task Should_Recommend_Based_On_Category_Preferences()
    {
        var result = await _handler.ExecuteAsync(new GetPersonalizedRecommendationsQuery(TestUserId));

        Assert.Single(result);
        Assert.Equal("Restaurant 2", result[0].EnName);
        var category = Assert.Single(result[0].Categories);
        Assert.Equal("Eastern", category.EnName);
    }

    [Fact]
    public async Task Should_Not_Recommend_Already_Reviewed_Businesses()
    {
        var result = await _handler.ExecuteAsync(new GetPersonalizedRecommendationsQuery(TestUserId));

        Assert.DoesNotContain(result, b => b.Id == "b1");
    }

    [Fact]
    public async Task Should_Include_Distance_When_Location_Provided()
    {
        var result = await _handler.ExecuteAsync(new GetPersonalizedRecommendationsQuery(
            TestUserId,
            new Location { Latitude = DamascusLatitude, Longitude = DamascusLongitude }));

        Assert.NotNull(result[0].DistanceInKm);
    }

    [Fact]
    public async Task Should_Not_Recommend_Deleted_Businesses()
    {
        var result = await _handler.ExecuteAsync(new GetPersonalizedRecommendationsQuery(TestUserId));

        Assert.DoesNotContain(result, b => b.ArName == "مطعم محذوف");
    }

    [Fact]
    public async Task Should_Respect_Limit_Parameter()
    {
        var result = await _handler.ExecuteAsync(new GetPersonalizedRecommendationsQuery(TestUserId, Limit: 1));

        Assert.Single(result);
    }
}