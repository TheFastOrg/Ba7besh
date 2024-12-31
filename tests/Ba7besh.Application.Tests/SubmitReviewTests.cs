using Ba7besh.Application.Exceptions;
using Ba7besh.Application.ReviewManagement;
using Dapper;
using Moq;

namespace Ba7besh.Application.Tests;

public class SubmitReviewTests : DatabaseTestBase
{
    private readonly SubmitReviewCommandHandler _handler;
    private readonly SubmitReviewCommandValidator _validator;
    private const string TestBusinessId = "test_business_id";
    private const string TestUserId = "test_user_id";

    public SubmitReviewTests(PostgresContainerFixture fixture) : base(fixture)
    {
        Mock<IPhotoStorageService> photoStorage = new();
        _handler = new SubmitReviewCommandHandler(Connection, photoStorage.Object);
        _validator = new SubmitReviewCommandValidator();
    }

    protected override async Task SeedTestData()
    {
        await Connection.ExecuteAsync("""
                                      INSERT INTO businesses (id, ar_name, en_name, location, country, type, status, slug, is_deleted)
                                      VALUES (@Id, 'مطعم للاختبار', 'Test Restaurant', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'test-restaurant', FALSE),
                                      (@DeletedId, 'مطعم محذوف', 'Deleted Restaurant', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'deleted-restaurant', TRUE)
                                      """,
            new { Id = TestBusinessId, DeletedId = "deleted_business" });
    }

    [Fact]
    public async Task Should_Submit_Basic_Review()
    {
        var command = new SubmitReviewCommand
        {
            BusinessId = TestBusinessId,
            UserId = TestUserId,
            OverallRating = 4,
            Content = "Great food!"
        };

        await _handler.HandleAsync(command);

        var review = await Connection.QuerySingleAsync(@"
            SELECT * FROM reviews 
            WHERE business_id = @BusinessId 
            AND user_id = @UserId",
            new { BusinessId = TestBusinessId, UserId = TestUserId });

        Assert.Equal(4, review.overall_rating);
        Assert.Equal("Great food!", review.content);
        Assert.Equal("pending", review.status);
    }

    [Fact]
    public async Task Should_Submit_Review_With_Dimension_Ratings()
    {
        var command = new SubmitReviewCommand
        {
            BusinessId = TestBusinessId,
            UserId = TestUserId,
            OverallRating = 4,
            Content = "Great food!",
            DimensionRatings = new[]
            {
                new ReviewDimensionRating(ReviewDimension.Taste, 5, "Amazing taste"),
                new ReviewDimensionRating(ReviewDimension.Service, 3, "Okay service")
            }
        };

        await _handler.HandleAsync(command);

        var dimensionRatings = (await Connection.QueryAsync(@"
            SELECT r.*, rr.dimension::text, rr.rating, rr.note
            FROM reviews r
            JOIN review_ratings rr ON r.id = rr.review_id
            WHERE r.business_id = @BusinessId 
            AND r.user_id = @UserId",
            new { BusinessId = TestBusinessId, UserId = TestUserId })).ToList();

        Assert.Equal(2, dimensionRatings.Count);
        Assert.Contains(dimensionRatings, r => r.dimension == "taste" && r.rating == 5M);
        Assert.Contains(dimensionRatings, r => r.dimension == "service" && r.rating == 3M);
    }

    [Fact]
    public async Task Should_Fail_When_Business_Not_Found()
    {
        var command = new SubmitReviewCommand
        {
            BusinessId = "nonexistent_id",
            UserId = TestUserId,
            OverallRating = 4
        };

        await Assert.ThrowsAsync<BusinessNotFoundException>(() =>
            _handler.HandleAsync(command));
    }

    [Fact]
    public async Task Should_Fail_When_Business_Is_Deleted()
    {
        var command = new SubmitReviewCommand
        {
            BusinessId = "deleted_business",
            UserId = TestUserId,
            OverallRating = 4
        };

        await Assert.ThrowsAsync<BusinessNotFoundException>(() =>
            _handler.HandleAsync(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Should_Validate_Overall_Rating_Range(decimal rating)
    {
        var command = new SubmitReviewCommand
        {
            BusinessId = TestBusinessId,
            UserId = TestUserId,
            OverallRating = rating
        };

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.OverallRating));
    }

    [Fact]
    public void Should_Validate_Dimension_Rating_Range()
    {
        var command = new SubmitReviewCommand
        {
            BusinessId = TestBusinessId,
            UserId = TestUserId,
            OverallRating = 4,
            DimensionRatings =
            [
                new ReviewDimensionRating(ReviewDimension.Taste, 6, null)
            ]
        };

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.DimensionRatings));
    }

    [Fact]
    public void Should_Validate_No_Duplicate_Dimensions()
    {
        var command = new SubmitReviewCommand
        {
            BusinessId = TestBusinessId,
            UserId = TestUserId,
            OverallRating = 4,
            DimensionRatings =
            [
                new ReviewDimensionRating(ReviewDimension.Taste, 4, null),
                new ReviewDimensionRating(ReviewDimension.Taste, 5, null)
            ]
        };

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.DimensionRatings));
    }
}