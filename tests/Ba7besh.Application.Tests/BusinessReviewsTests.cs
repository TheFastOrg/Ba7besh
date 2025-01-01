using Ba7besh.Application.Exceptions;
using Ba7besh.Application.ReviewManagement;

namespace Ba7besh.Application.Tests;

public class BusinessReviewsTests : DatabaseTestBase
{
    private readonly GetBusinessReviewsQueryHandler _handler;
    private const string TestBusinessId = "test_business_id";
    private const string TestUserId = "test_user_id";

    public BusinessReviewsTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _handler = new GetBusinessReviewsQueryHandler(Connection);
    }

    protected override async Task SeedTestData()
    {
        await Connection.ExecuteAsync($"""
            INSERT INTO businesses (id, ar_name, en_name, location, country, type, status, slug, is_deleted)
            VALUES 
            ('{TestBusinessId}', 'مطعم للاختبار', 'Test Restaurant', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'test-restaurant', FALSE),
            ('deleted_business', 'مطعم محذوف', 'Deleted Restaurant', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'deleted-restaurant', TRUE)
            """);

        await Connection.ExecuteAsync($"""
            INSERT INTO reviews (id, business_id, user_id, overall_rating, content, status, created_at, is_deleted)
            VALUES 
            ('r1', '{TestBusinessId}', '{TestUserId}', 4.5, 'Great food!', 'approved', CURRENT_TIMESTAMP - INTERVAL '2 days', FALSE),
            ('r2', '{TestBusinessId}', 'user2', 3.5, 'Good food!', 'approved', CURRENT_TIMESTAMP - INTERVAL '1 day', FALSE),
            ('r3', '{TestBusinessId}', 'user3', 5.0, 'Excellent!', 'pending', CURRENT_TIMESTAMP, FALSE),
            ('r4', '{TestBusinessId}', 'user4', 4.0, 'Deleted review', 'approved', CURRENT_TIMESTAMP, TRUE)
            """);

        await Connection.ExecuteAsync("""
            INSERT INTO review_ratings (review_id, dimension, rating, note)
            VALUES
            ('r1', 'taste', 5, 'Amazing taste'),
            ('r1', 'service', 4, 'Good service'),
            ('r2', 'taste', 3, 'Average taste')
            """);

        await Connection.ExecuteAsync("""
            INSERT INTO review_reactions (id, review_id, user_id, reaction, is_deleted)
            VALUES
            ('rr1', 'r1', 'user5', 'helpful', FALSE),
            ('rr2', 'r1', 'user6', 'helpful', FALSE),
            ('rr3', 'r1', 'user7', 'unhelpful', FALSE),
            ('rr4', 'r2', 'user8', 'helpful', FALSE),
            ('rr5', 'r2', 'user9', 'helpful', TRUE)
            """);

        await Connection.ExecuteAsync("""
            INSERT INTO review_photos (id, review_id, photo_url, description, mime_type, size_bytes, created_at, is_deleted)
            VALUES
            ('p1', 'r1', 'https://example.com/photo1.jpg', 'Food photo', 'image/jpeg', 1024, CURRENT_TIMESTAMP - INTERVAL '1 hour', FALSE),
            ('p2', 'r1', 'https://example.com/photo2.jpg', 'Interior photo', 'image/jpeg', 2048, CURRENT_TIMESTAMP, FALSE),
            ('p3', 'r1', 'https://example.com/deleted.jpg', 'Deleted photo', 'image/jpeg', 512, CURRENT_TIMESTAMP, TRUE),
            ('p4', 'r2', 'https://example.com/photo3.jpg', 'Another photo', 'image/jpeg', 1536, CURRENT_TIMESTAMP, FALSE)
            """);
    }

    [Fact]
    public async Task Should_Return_Reviews_With_All_Details()
    {
        var result = await _handler.ExecuteAsync(new GetBusinessReviewsQuery(TestBusinessId));

        Assert.Equal(2, result.Reviews.Count);
        var review = result.Reviews.First(r => r.Id == "r1");

        // Basic review details
        Assert.Equal(4.5m, review.OverallRating);
        Assert.Equal("Great food!", review.Content);
        Assert.Equal(TestUserId, review.ReviewerName);

        // Dimension ratings
        Assert.Equal(2, review.DimensionRatings.Count);
        var tasteRating = Assert.Single(review.DimensionRatings.Where(r => r.Dimension == ReviewDimension.Taste));
        Assert.Equal(5m, tasteRating.Rating);
        Assert.Equal("Amazing taste", tasteRating.Note);

        // Reactions
        Assert.Equal(2, review.ReactionsSummary.HelpfulCount);
        Assert.Equal(1, review.ReactionsSummary.UnhelpfulCount);

        // Photos
        Assert.Equal(2, review.Photos.Count);
        Assert.Contains(review.Photos, photo => photo.PhotoUrl == "https://example.com/photo2.jpg");
        Assert.Contains(review.Photos, photo => photo.Description == "Interior photo");
    }

    [Fact]
    public async Task Should_Return_Reviews_In_Chronological_Order()
    {
        var result = await _handler.ExecuteAsync(new GetBusinessReviewsQuery(TestBusinessId));

        Assert.Equal(2, result.Reviews.Count);
        Assert.Equal("r2", result.Reviews[0].Id);
        Assert.Equal("r1", result.Reviews[1].Id);
    }

    [Fact]
    public async Task Should_Support_Pagination()
    {
        var result = await _handler.ExecuteAsync(new GetBusinessReviewsQuery(TestBusinessId)
        {
            PageSize = 1,
            PageNumber = 2
        });

        Assert.Equal(2, result.TotalCount);
        Assert.Single(result.Reviews);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal("r1", result.Reviews[0].Id);
    }

    [Fact]
    public async Task Should_Not_Return_Pending_Or_Deleted_Reviews()
    {
        var result = await _handler.ExecuteAsync(new GetBusinessReviewsQuery(TestBusinessId));

        Assert.DoesNotContain(result.Reviews, r => r.Content == "Excellent!");
        Assert.DoesNotContain(result.Reviews, r => r.Content == "Deleted review");
    }

    [Fact]
    public async Task Should_Not_Return_Deleted_Photos()
    {
        var result = await _handler.ExecuteAsync(new GetBusinessReviewsQuery(TestBusinessId));

        var review = result.Reviews.First(r => r.Id == "r1");
        Assert.DoesNotContain(review.Photos, p => p.Description == "Deleted photo");
    }

    [Fact]
    public async Task Should_Throw_When_Business_Not_Found()
    {
        await Assert.ThrowsAsync<BusinessNotFoundException>(() =>
            _handler.ExecuteAsync(new GetBusinessReviewsQuery("nonexistent_id")));
    }

    [Fact]
    public async Task Should_Throw_When_Business_Is_Deleted()
    {
        await Assert.ThrowsAsync<BusinessNotFoundException>(() =>
            _handler.ExecuteAsync(new GetBusinessReviewsQuery("deleted_business")));
    }
}