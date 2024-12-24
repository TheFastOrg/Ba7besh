using Ba7besh.Application.TagManagement;
using Moq;

namespace Ba7besh.Application.Tests;

public class GetTagsTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly GetTagsQueryHandler _handler;

    public GetTagsTests()
    {
        _tagRepositoryMock = new Mock<ITagRepository>();
        _handler = new GetTagsQueryHandler(_tagRepositoryMock.Object);
    }

    [Fact]
    public async Task Should_Return_All_Tags()
    {
        // Arrange
        var expectedTags = new List<string>
        {
            "Fast Food",
            "Pizza",
            "Burger",
            "Healthy"
        };

        _tagRepositoryMock
            .Setup(r => r.GetTagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTags);

        // Act
        var result = await _handler.ExecuteAsync(new GetTagsQuery());

        // Assert
        Assert.Equal(expectedTags.Count, result.Count);
        Assert.Equal(expectedTags, result);
    }

    [Fact]
    public async Task Should_Handle_Empty_Tags_List()
    {
        // Arrange
        _tagRepositoryMock
            .Setup(r => r.GetTagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());

        // Act
        var result = await _handler.ExecuteAsync(new GetTagsQuery());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Should_Call_Repository_Once()
    {
        // Arrange
        _tagRepositoryMock
            .Setup(r => r.GetTagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());

        // Act
        await _handler.ExecuteAsync(new GetTagsQuery());

        // Assert
        _tagRepositoryMock.Verify(
            r => r.GetTagsAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}