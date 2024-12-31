using Ba7besh.Application.ReviewManagement;

namespace Ba7besh.Application.Tests;

public class GetRecentReviewsTests : DatabaseTestBase
{
    private readonly GetRecentReviewsQueryHandler _handler;

    public GetRecentReviewsTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _handler = new GetRecentReviewsQueryHandler(Connection);
    }

    protected override async Task SeedTestData()
    {
        await Connection.ExecuteAsync(@"
            INSERT INTO businesses (id, ar_name, en_name, location, country, type, status, slug, city, is_deleted) VALUES 
            ('b1', 'مطعم 1', 'Restaurant 1', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-1', 'Damascus', FALSE),
            ('b2', 'مطعم 2', 'Restaurant 2', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-2', 'Aleppo', FALSE)
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO reviews (id, business_id, user_id, overall_rating, content, status, created_at, is_deleted) VALUES 
            ('r1', 'b1', 'u1', 4.5, 'Great!', 'approved', CURRENT_TIMESTAMP - INTERVAL '1 day', FALSE),
            ('r2', 'b2', 'u2', 5.0, 'Amazing!', 'approved', CURRENT_TIMESTAMP, FALSE),
            ('r3', 'b1', 'u3', 3.5, 'Good', 'pending', CURRENT_TIMESTAMP, FALSE)
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO review_ratings (review_id, dimension, rating, note) VALUES 
            ('r1', 'taste', 5, 'Excellent taste'),
            ('r1', 'service', 4, 'Good service'),
            ('r2', 'taste', 5, 'Perfect'),
            ('r2', 'service', 4, 'Good service')
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO review_reactions (id, review_id, user_id, reaction, is_deleted) VALUES 
            ('rr1', 'r1', 'u4', 'helpful', FALSE),
            ('rr2', 'r1', 'u5', 'helpful', FALSE),
            ('rr3', 'r1', 'u6', 'unhelpful', FALSE)
        ");
    }

    [Fact]
    public async Task Should_Return_Recent_Reviews_In_Chronological_Order()
    {
        var reviews = await _handler.ExecuteAsync(new GetRecentReviewsQuery());

        Assert.Equal(2, reviews.Count);
        var firstReview = reviews[0];
        Assert.Equal("Restaurant 2", firstReview.BusinessEnName);
        Assert.Equal(5.0m, firstReview.OverallRating);
    }

    [Fact]
    public async Task Should_Include_Dimension_Ratings()
    {
        var reviews = await _handler.ExecuteAsync(new GetRecentReviewsQuery());

        var review = reviews.First(r => r.Id == "r1");
        Assert.Equal(2, review.DimensionRatings.Count);
        Assert.Contains(review.DimensionRatings, 
            r => r is { Dimension: ReviewDimension.Taste, Rating: 5 });
        Assert.Contains(review.DimensionRatings,
            r => r is { Dimension: ReviewDimension.Service, Rating: 4 });
    }

    [Fact]
    public async Task Should_Include_Reaction_Counts()
    {
        var reviews = await _handler.ExecuteAsync(new GetRecentReviewsQuery());

        var review = reviews.First(r => r.Id == "r1");
        Assert.Equal(2, review.ReactionsSummary.HelpfulCount);
        Assert.Equal(1, review.ReactionsSummary.UnhelpfulCount);
    }

    [Fact]
    public async Task Should_Not_Return_Pending_Or_Deleted_Reviews()
    {
        var reviews = await _handler.ExecuteAsync(new GetRecentReviewsQuery());

        Assert.DoesNotContain(reviews, r => r.Content == "Good");
    }
}