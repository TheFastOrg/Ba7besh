using Ba7besh.Application.ReviewManagement;
using Dapper;

namespace Ba7besh.Application.Tests;

public class ReviewCommentsTests : DatabaseTestBase
{
    private readonly AddReviewCommentCommandHandler _handler;
    private readonly GetReviewCommentsQueryHandler _queryHandler;
    private readonly AddReviewCommentCommandValidator _validator;
    private const string TestReviewId = "test_review_id";
    private const string TestUserId = "test_user_id";

    public ReviewCommentsTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _handler = new AddReviewCommentCommandHandler(Connection);
        _queryHandler = new GetReviewCommentsQueryHandler(Connection);
        _validator = new AddReviewCommentCommandValidator();
    }

    protected override async Task SeedTestData()
    {
        await Connection.ExecuteAsync("""
            INSERT INTO businesses (id, ar_name, en_name, location, country, type, status, slug, is_deleted)
            VALUES ('test_business', 'مطعم للاختبار', 'Test Restaurant', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'test-restaurant', FALSE)
            """);

        await Connection.ExecuteAsync($"""
            INSERT INTO reviews (id, business_id, user_id, overall_rating, content, status, created_at, is_deleted)
            VALUES ('{TestReviewId}', 'test_business', '{TestUserId}', 4, 'Great food!', 'approved', CURRENT_TIMESTAMP, FALSE)
            """);
    }

    [Fact]
    public async Task Should_Add_Comment()
    {
        var command = new AddReviewCommentCommand(TestReviewId, TestUserId, "Nice review!");
        await _handler.HandleAsync(command);

        var result = await _queryHandler.ExecuteAsync(new GetReviewCommentsQuery(TestReviewId));
        
        var comment = Assert.Single(result.Comments);
        Assert.Equal(TestUserId, comment.UserId);
        Assert.Equal("Nice review!", comment.Content);
    }

    [Fact]
    public async Task Should_Return_Comments_In_Chronological_Order()
    {
        var command1 = new AddReviewCommentCommand(TestReviewId, "user1", "First comment");
        var command2 = new AddReviewCommentCommand(TestReviewId, "user2", "Second comment");
        await _handler.HandleAsync(command1);
        await _handler.HandleAsync(command2);

        var result = await _queryHandler.ExecuteAsync(new GetReviewCommentsQuery(TestReviewId));

        Assert.Equal(2, result.Comments.Count);
        Assert.Equal("Second comment", result.Comments[0].Content);
        Assert.Equal("First comment", result.Comments[1].Content);
    }

    [Fact]
    public async Task Should_Support_Pagination()
    {
        for (var i = 1; i <= 5; i++)
        {
            await _handler.HandleAsync(new AddReviewCommentCommand(TestReviewId, TestUserId, $"Comment {i}"));
        }

        var result = await _queryHandler.ExecuteAsync(new GetReviewCommentsQuery(TestReviewId)
        {
            PageSize = 2,
            PageNumber = 2
        });

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Comments.Count);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.PageNumber);
    }

    [Fact]
    public async Task Should_Not_Return_Deleted_Comments()
    {
        await _handler.HandleAsync(new AddReviewCommentCommand(TestReviewId, TestUserId, "Test comment"));
        
        await Connection.ExecuteAsync("""
            UPDATE review_comments 
            SET is_deleted = TRUE 
            WHERE review_id = @ReviewId
            """,
            new { ReviewId = TestReviewId });

        var result = await _queryHandler.ExecuteAsync(new GetReviewCommentsQuery(TestReviewId));
        
        Assert.Empty(result.Comments);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Validate_Empty_Content(string content)
    {
        var command = new AddReviewCommentCommand(TestReviewId, TestUserId, content);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Content));
    }

    [Fact]
    public void Should_Validate_Content_Length()
    {
        var command = new AddReviewCommentCommand(TestReviewId, TestUserId, new string('x', 1001));
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Content));
    }
}