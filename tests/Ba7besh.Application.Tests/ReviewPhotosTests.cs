using Ba7besh.Application.ReviewManagement;
using Dapper;
using Moq;

namespace Ba7besh.Application.Tests;

public class ReviewPhotosTests : DatabaseTestBase
{
    private readonly SubmitReviewCommandHandler _handler;
    private readonly Mock<IPhotoStorageService> _photoStorage;
    private readonly SubmitReviewCommandValidator _validator;
    private const string TestBusinessId = "test_business_id";
    private const string TestUserId = "test_user_id";

    public ReviewPhotosTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _photoStorage = new Mock<IPhotoStorageService>();
        _handler = new SubmitReviewCommandHandler(Connection, _photoStorage.Object);
        _validator = new SubmitReviewCommandValidator();
    }

    protected override async Task SeedTestData()
    {
        await Connection.ExecuteAsync("""
            INSERT INTO businesses (id, ar_name, en_name, location, country, type, status, slug, is_deleted)
            VALUES (@Id, 'مطعم للاختبار', 'Test Restaurant', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'test-restaurant', FALSE)
            """,
            new { Id = TestBusinessId });
    }

    [Fact]
    public async Task Should_Upload_Photos_With_Review()
    {
        // Arrange
        const string photoUrl = "https://storage.test/photo.jpg";
        _photoStorage.Setup(x => x.UploadPhotoAsync(
                It.IsAny<string>(), 
                It.IsAny<Stream>(), 
                It.IsAny<string>()))
            .ReturnsAsync(photoUrl);

        var mockFile = CreateMockFile("test.jpg", "image/jpeg", 1024, "Nice atmosphere");
        var command = new SubmitReviewCommand
        {
            BusinessId = TestBusinessId,
            UserId = TestUserId,
            OverallRating = 4,
            Content = "Great food!",
            Photos =
            [
                mockFile.Object
            ]
        };

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _photoStorage.Verify(x => x.UploadPhotoAsync(
            It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<string>()), Times.Once);

        var photos = await Connection.QueryAsync<ReviewPhoto>(
            "SELECT * FROM review_photos WHERE review_id IN (SELECT id FROM reviews WHERE business_id = @BusinessId)",
            new { BusinessId = TestBusinessId });

        var photo = Assert.Single(photos);
        Assert.Equal(photoUrl, photo.PhotoUrl);
        Assert.Equal("Nice atmosphere", photo.Description);
        Assert.Equal("image/jpeg", photo.MimeType);
        Assert.Equal(1024, photo.SizeBytes);
    }

    [Fact]
    public void Should_Validate_Photo_Count()
    {
        var mockFiles = Enumerable.Range(0, 6)
            .Select(_ => CreateMockFile("test.jpg", "image/jpeg", 1024).Object);

        var command = new SubmitReviewCommand
        {
            BusinessId = TestBusinessId,
            UserId = TestUserId,
            OverallRating = 4,
            Photos = mockFiles.ToList()
        };

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Photos");
    }

    [Theory]
    [InlineData("text/plain")]
    [InlineData("image/gif")]
    public void Should_Validate_Photo_Content_Type(string contentType)
    {
        var command = new SubmitReviewCommand
        {
            BusinessId = TestBusinessId,
            UserId = TestUserId,
            OverallRating = 4,
            Photos =
            [
                CreateMockFile("test.jpg", contentType, 1024).Object
            ]
        };

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Photos[0].ContentType");
    }

    [Fact]
    public void Should_Validate_Photo_Size()
    {
        var command = new SubmitReviewCommand
        {
            BusinessId = TestBusinessId,
            UserId = TestUserId,
            OverallRating = 4,
            Photos =
            [
                CreateMockFile("test.jpg", "image/jpeg", 6 * 1024 * 1024).Object
            ]
        };

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Photos[0].Length");
    }

    [Fact]
    public void Should_Validate_Photo_Description_Length()
    {
        var command = new SubmitReviewCommand
        {
            BusinessId = TestBusinessId,
            UserId = TestUserId,
            OverallRating = 4,
            Photos =
            [
                CreateMockFile("test.jpg", "image/jpeg", 1024, new string('x', 501)).Object
            ]
        };

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Photos[0].Description");
    }

    private static Mock<IReviewPhotoUpload> CreateMockFile(string fileName, string contentType, long length, string? description = null)
    {
        var mockFile = new Mock<IReviewPhotoUpload>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Description).Returns(description);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        return mockFile;
    }
}