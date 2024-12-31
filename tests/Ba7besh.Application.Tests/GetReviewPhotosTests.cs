using Ba7besh.Application.Exceptions;
using Ba7besh.Application.ReviewManagement;

namespace Ba7besh.Application.Tests;

public class GetReviewPhotosTests : DatabaseTestBase
{
    private readonly GetReviewPhotosQueryHandler _handler;
    private const string TestReviewId = "test_review_id";

    public GetReviewPhotosTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _handler = new GetReviewPhotosQueryHandler(Connection);
    }

    protected override async Task SeedTestData()
    {
        await Connection.ExecuteAsync("""
            INSERT INTO businesses (id, ar_name, en_name, location, country, type, status, slug, is_deleted)
            VALUES ('test_business', 'مطعم للاختبار', 'Test Restaurant', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'test-restaurant', FALSE)
            """);

        await Connection.ExecuteAsync($"""
            INSERT INTO reviews (id, business_id, user_id, overall_rating, content, status, created_at, is_deleted)
            VALUES ('{TestReviewId}', 'test_business', 'test_user', 4, 'Great food!', 'approved', CURRENT_TIMESTAMP, FALSE)
            """);

        await Connection.ExecuteAsync($"""
            INSERT INTO review_photos (id, review_id, photo_url, description, mime_type, size_bytes, created_at, is_deleted)
            VALUES 
            ('p1', '{TestReviewId}', 'https://storage.test/photo1.jpg', 'Restaurant front', 'image/jpeg', 1024, CURRENT_TIMESTAMP - INTERVAL '1 hour', FALSE),
            ('p2', '{TestReviewId}', 'https://storage.test/photo2.jpg', 'Food', 'image/jpeg', 2048, CURRENT_TIMESTAMP, FALSE),
            ('p3', '{TestReviewId}', 'https://storage.test/deleted.jpg', 'Deleted photo', 'image/jpeg', 512, CURRENT_TIMESTAMP, TRUE)
            """);
    }

    [Fact]
    public async Task Should_Return_Photos_In_Chronological_Order()
    {
        var photos = await _handler.ExecuteAsync(new GetReviewPhotosQuery(TestReviewId));

        Assert.Equal(2, photos.Count);
        Assert.Equal("https://storage.test/photo2.jpg", photos[0].PhotoUrl);
        Assert.Equal("https://storage.test/photo1.jpg", photos[1].PhotoUrl);
    }

    [Fact]
    public async Task Should_Not_Return_Deleted_Photos()
    {
        var photos = await _handler.ExecuteAsync(new GetReviewPhotosQuery(TestReviewId));

        Assert.DoesNotContain(photos, p => p.Description == "Deleted photo");
    }

    [Fact]
    public async Task Should_Return_Empty_List_For_Review_Without_Photos()
    {
        const string reviewIdWithoutPhotos = "review_without_photos";
        await Connection.ExecuteAsync($"""
            INSERT INTO reviews (id, business_id, user_id, overall_rating, content, status, created_at, is_deleted)
            VALUES ('{reviewIdWithoutPhotos}', 'test_business', 'test_user', 4, 'No photos!', 'approved', CURRENT_TIMESTAMP, FALSE)
            """);

        var photos = await _handler.ExecuteAsync(new GetReviewPhotosQuery(reviewIdWithoutPhotos));

        Assert.Empty(photos);
    }

    [Fact]
    public async Task Should_Throw_When_Review_Not_Found()
    {
        await Assert.ThrowsAsync<ReviewNotFoundException>(() =>
            _handler.ExecuteAsync(new GetReviewPhotosQuery("nonexistent_review")));
    }
}