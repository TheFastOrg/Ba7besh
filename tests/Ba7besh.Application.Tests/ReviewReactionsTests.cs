using Ba7besh.Application.ReviewManagement;

namespace Ba7besh.Application.Tests;

public class ReviewReactionsTests : DatabaseTestBase
{
    private readonly ReactToReviewCommandHandler _handler;
    private readonly GetReviewReactionsSummaryQueryHandler _queryHandler;
    private const string TestReviewId = "test_review_id";
    private const string TestUserId = "test_user_id";

    public ReviewReactionsTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _handler = new ReactToReviewCommandHandler(Connection);
        _queryHandler = new GetReviewReactionsSummaryQueryHandler(Connection);
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
    public async Task Should_Add_Helpful_Reaction()
    {
        var helpfulReviewCommand = new ReactToReviewCommand(TestReviewId, TestUserId, ReviewReaction.Helpful);
        await _handler.HandleAsync(helpfulReviewCommand);

        var summary = await _queryHandler.ExecuteAsync(
            new GetReviewReactionsSummaryQuery(TestReviewId, TestUserId));

        Assert.Equal(1, summary.HelpfulCount);
        Assert.Equal(0, summary.UnhelpfulCount);
        Assert.Equal(ReviewReaction.Helpful, summary.UserReaction);
    }

    [Fact]
    public async Task Should_Change_Reaction_Type()
    {
        var helpfulReviewCommand = new ReactToReviewCommand(TestReviewId, TestUserId, ReviewReaction.Helpful);
        await _handler.HandleAsync(helpfulReviewCommand);

        var unhelpfulReviewCommand = new ReactToReviewCommand(TestReviewId, TestUserId, ReviewReaction.Unhelpful);
        await _handler.HandleAsync(unhelpfulReviewCommand);

        var summary = await _queryHandler.ExecuteAsync(
            new GetReviewReactionsSummaryQuery(TestReviewId, TestUserId));

        Assert.Equal(0, summary.HelpfulCount);
        Assert.Equal(1, summary.UnhelpfulCount);
        Assert.Equal(ReviewReaction.Unhelpful, summary.UserReaction);
    }

    [Fact]
    public async Task Should_Return_Null_UserReaction_When_No_Reaction()
    {
        var summary = await _queryHandler.ExecuteAsync(
            new GetReviewReactionsSummaryQuery(TestReviewId, TestUserId));

        Assert.Equal(0, summary.HelpfulCount);
        Assert.Equal(0, summary.UnhelpfulCount);
        Assert.Null(summary.UserReaction);
    }

    [Fact]
    public async Task Should_Count_Multiple_User_Reactions()
    {
        await _handler.HandleAsync(new ReactToReviewCommand(TestReviewId, "user1", ReviewReaction.Helpful));
        await _handler.HandleAsync(new ReactToReviewCommand(TestReviewId, "user2", ReviewReaction.Helpful));
        await _handler.HandleAsync(new ReactToReviewCommand(TestReviewId, "user3", ReviewReaction.Unhelpful));
        
        var summary = await _queryHandler.ExecuteAsync(
            new GetReviewReactionsSummaryQuery(TestReviewId));

        Assert.Equal(2, summary.HelpfulCount);
        Assert.Equal(1, summary.UnhelpfulCount);
    }
}