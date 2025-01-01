using Ba7besh.Application.BusinessDiscovery;
using Dapper;
using Npgsql;

namespace Ba7besh.Application.Tests;

public class SuggestBusinessTests : DatabaseTestBase
{
    private readonly SuggestBusinessCommandHandler _handler;
    private readonly SuggestBusinessCommandValidator _validator;
    private const string TestUserId = "test_user_id";

    public SuggestBusinessTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _handler = new SuggestBusinessCommandHandler(Connection);
        _validator = new SuggestBusinessCommandValidator();
    }

    protected override async Task SeedTestData()
    {
        await Connection.ExecuteAsync("""
            INSERT INTO suggested_businesses (id, user_id, ar_name, en_name, location, description, status)
            VALUES ('existing', 'other_user', 'مطعم موجود', 'Existing Restaurant', 
                    ST_MakePoint(36.2765, 33.5138), 'Test description', 'pending')
            """);
    }

    [Fact]
    public async Task Should_Submit_Valid_Suggestion()
    {
        var command = new SuggestBusinessCommand
        {
            UserId = TestUserId,
            ArName = "مطعم للاختبار",
            EnName = "Test Restaurant",
            Location = new Location { Latitude = 33.5138, Longitude = 36.2765 },
            Description = "A great restaurant"
        };

        await _handler.HandleAsync(command);

        var suggestion = await Connection.QuerySingleAsync<dynamic>(
            """
            SELECT ar_name, en_name, description, status
            FROM suggested_businesses WHERE user_id = @UserId 
            """,
            new { UserId = TestUserId });

        Assert.Equal("مطعم للاختبار", suggestion.ar_name);
        Assert.Equal("Test Restaurant", suggestion.en_name);
        Assert.Equal("A great restaurant", suggestion.description);
        Assert.Equal("pending", suggestion.status.ToString());
    }

    [Fact]
    public async Task Should_Prevent_Duplicate_Names()
    {
        var command = new SuggestBusinessCommand
        {
            UserId = TestUserId,
            ArName = "مطعم موجود",
            EnName = "Existing Restaurant",
            Location = new Location { Latitude = 33.5138, Longitude = 36.2765 },
            Description = "Test description"
        };

        await Assert.ThrowsAsync<PostgresException>(() => _handler.HandleAsync(command));
    }

    [Theory]
    [InlineData("", "Test Restaurant", "Invalid Arabic name")]
    [InlineData("مطعم للاختبار", "", "Invalid English name")]
    [InlineData("مطعم للاختبار", "Test Restaurant", "")]
    public void Should_Validate_Required_Fields(string arName, string enName, string description)
    {
        var command = new SuggestBusinessCommand
        {
            UserId = TestUserId,
            ArName = arName,
            EnName = enName,
            Location = new Location { Latitude = 33.5138, Longitude = 36.2765 },
            Description = description
        };

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(91)]
    [InlineData(-91)]
    [InlineData(0, 181)]
    [InlineData(0, -181)]
    public void Should_Validate_Coordinates(double latitude = 0, double longitude = 0)
    {
        var command = new SuggestBusinessCommand
        {
            UserId = TestUserId,
            ArName = "مطعم للاختبار",
            EnName = "Test Restaurant",
            Location = new Location { Latitude = latitude, Longitude = longitude },
            Description = "Test description"
        };

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Location");
    }
}