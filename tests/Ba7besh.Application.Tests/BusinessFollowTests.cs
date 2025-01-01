using Ba7besh.Application.BusinessDiscovery;

namespace Ba7besh.Application.Tests;

public class BusinessFollowTests : DatabaseTestBase
{
    private readonly FollowBusinessCommandHandler _followHandler;
    private readonly UnfollowBusinessCommandHandler _unfollowHandler;
    private readonly GetFollowStatusQueryHandler _getStatusHandler;
    private const string TestBusinessId = "test_business_id";
    private const string TestBusinessId2 = "test_business_id_2";
    private const string TestUserId = "test_user_id";

    public BusinessFollowTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _followHandler = new FollowBusinessCommandHandler(Connection);
        _unfollowHandler = new UnfollowBusinessCommandHandler(Connection);
        _getStatusHandler = new GetFollowStatusQueryHandler(Connection);
    }

    protected override async Task SeedTestData()
    {
        await Connection.ExecuteAsync($"""
                                       INSERT INTO businesses (id, ar_name, en_name, location, country, type, status, slug, is_deleted)
                                       VALUES 
                                       ('{TestBusinessId}', 'مطعم 1', 'Restaurant 1', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-1', FALSE),
                                       ('{TestBusinessId2}', 'مطعم 2', 'Restaurant 2', ST_MakePoint(0, 0), 'SY', 'restaurant', 'active', 'restaurant-2', FALSE)
                                       """);
    }

    [Fact]
    public async Task Should_Follow_Business()
    {
        var command = new FollowBusinessCommand(TestBusinessId, TestUserId);
        await _followHandler.HandleAsync(command);

        var isFollowing = await _getStatusHandler.ExecuteAsync(
            new GetFollowStatusQuery(TestBusinessId, TestUserId));
        Assert.True(isFollowing);
    }

    [Fact]
    public async Task Should_Unfollow_Business()
    {
        await _followHandler.HandleAsync(new FollowBusinessCommand(TestBusinessId, TestUserId));
        await _unfollowHandler.HandleAsync(new UnfollowBusinessCommand(TestBusinessId, TestUserId));

        var isFollowing = await _getStatusHandler.ExecuteAsync(
            new GetFollowStatusQuery(TestBusinessId, TestUserId));
        Assert.False(isFollowing);
    }

    [Fact]
    public async Task Should_Support_Follow_After_Unfollow()
    {
        await _followHandler.HandleAsync(new FollowBusinessCommand(TestBusinessId, TestUserId));
        await _unfollowHandler.HandleAsync(new UnfollowBusinessCommand(TestBusinessId, TestUserId));
        await _followHandler.HandleAsync(new FollowBusinessCommand(TestBusinessId, TestUserId));

        var isFollowing = await _getStatusHandler.ExecuteAsync(
            new GetFollowStatusQuery(TestBusinessId, TestUserId));
        Assert.True(isFollowing);
    }
}