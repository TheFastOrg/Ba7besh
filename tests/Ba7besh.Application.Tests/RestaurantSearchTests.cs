using Ba7besh.Application.RestaurantDiscovery;
using Moq;

namespace Ba7besh.Application.Tests;

public class RestaurantSearchTests
{
    private readonly Mock<IRestaurantSearchService> _searchServiceMock;
    private readonly SearchRestaurantsQueryHandler _handler;


    public RestaurantSearchTests()
    {
        _searchServiceMock = new Mock<IRestaurantSearchService>();
        _handler = new SearchRestaurantsQueryHandler(_searchServiceMock.Object);
    }

    [Fact]
    public async Task Should_Return_Restaurant_With_Matching_Arabic_Name()
    {
        // Arrange
        var expectedRestaurant = new RestaurantSummary
        {
            Id = "1",
            ArName = "مطعم الشام",
            EnName = "Damascus Restaurant",
            Location = "location1",
            City = "damascus",
            Type = "restaurant"
        };

        _searchServiceMock.Setup(s =>
                s.SearchAsync(
                    It.Is<SearchRestaurantsQuery>(q => q.SearchTerm == "الشام"),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new SearchRestaurantsResult
            {
                Restaurants = [expectedRestaurant],
                TotalCount = 1,
                PageSize = 20,
                PageNumber = 1
            });

        // Act
        var result = await _handler.ExecuteAsync(new SearchRestaurantsQuery { SearchTerm = "الشام" });

        // Assert
        var restaurant = Assert.Single(result.Restaurants);
        Assert.Equal(expectedRestaurant.ArName, restaurant.ArName);
    }

    [Fact]
    public async Task Should_Return_Restaurant_Categories_With_Arabic_And_English_Names()
    {
        // Arrange
        var expectedRestaurant = new RestaurantSummary
        {
            Id = "2",
            ArName = "برغر",
            EnName = "Burger Place",
            Location = "location2",
            City = "damascus",
            Type = "restaurant",
            Categories =
            [
                new CategoryInfo()
                {
                    Id = "cat1",
                    ArName = "وجبات سريعة",
                    EnName = "Fast Food"
                }
            ]
        };

        _searchServiceMock.Setup(s =>
                s.SearchAsync(
                    It.Is<SearchRestaurantsQuery>(q => q.SearchTerm == "Burger"),
                    It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new SearchRestaurantsResult
            {
                Restaurants = [expectedRestaurant],
                TotalCount = 1,
                PageSize = 20,
                PageNumber = 1
            });

        // Act
        var result = await _handler.ExecuteAsync(new SearchRestaurantsQuery { SearchTerm = "Burger" });

        // Assert
        var restaurant = Assert.Single(result.Restaurants);
        var category = Assert.Single(restaurant.Categories);
        Assert.Equal("وجبات سريعة", category.ArName);
        Assert.Equal("Fast Food", category.EnName);
    }

    [Fact]
    public async Task Should_Support_Pagination()
    {
        // Arrange
        RestaurantSummary[] restaurants =
        [
            new()
            {
                Id = "1",
                ArName = "مطعم 1",
                EnName = "Restaurant 1",
                Location = "loc1",
                City = "damascus",
                Type = "restaurant"
            },
            new()
            {
                Id = "2",
                ArName = "مطعم 2",
                EnName = "Restaurant 2",
                Location = "loc2",
                City = "damascus",
                Type = "restaurant"
            }
        ];

        _searchServiceMock.Setup(s =>
                s.SearchAsync(
                    It.Is<SearchRestaurantsQuery>(q => q.PageSize == 2 && q.PageNumber == 1),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchRestaurantsResult
            {
                Restaurants = restaurants,
                TotalCount = 5,
                PageSize = 2,
                PageNumber = 1
            });

        // Act
        var result = await _handler.ExecuteAsync(new SearchRestaurantsQuery { PageSize = 2, PageNumber = 1 });

        // Assert
        Assert.Equal(2, result.Restaurants.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task Should_Return_Empty_Result_When_No_Matches()
    {
        // Arrange
        _searchServiceMock.Setup(s =>
                s.SearchAsync(
                    It.Is<SearchRestaurantsQuery>(q => q.SearchTerm == "NonExistent"),
                    It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new SearchRestaurantsResult
            {
                Restaurants = Array.Empty<RestaurantSummary>(),
                TotalCount = 0,
                PageSize = 20,
                PageNumber = 1
            });

        // Act
        var result = await _handler.ExecuteAsync(new SearchRestaurantsQuery { SearchTerm = "NonExistent" });

        // Assert
        Assert.Empty(result.Restaurants);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact] 
    public async Task Should_Return_Restaurants_With_Matching_Tags()
    {
        // Arrange
        var expectedRestaurant = new RestaurantSummary
        {
            Id = "1",
            ArName = "مطعم الشام",
            EnName = "Damascus Restaurant",
            Location = "location1",
            City = "damascus",
            Type = "restaurant",
            Tags = ["Pizza", "Salads"]
        };

        _searchServiceMock.Setup(s =>
                s.SearchAsync(
                    It.Is<SearchRestaurantsQuery>(q => 
                        q.Tags!.Contains("Pizza") && 
                        q.Tags!.Contains("Salads")),
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new SearchRestaurantsResult
            {
                Restaurants = [expectedRestaurant],
                TotalCount = 1,
                PageSize = 20,
                PageNumber = 1
            });

        // Act
        var result = await _handler.ExecuteAsync(new SearchRestaurantsQuery 
        { 
            Tags = ["Pizza", "Salads"] 
        });

        // Assert
        var restaurant = Assert.Single(result.Restaurants);
        Assert.Contains("Pizza", restaurant.Tags);
        Assert.Contains("Salads", restaurant.Tags);
    }
    
    [Fact]
    public async Task Should_Pass_Search_Parameters_To_Service_Correctly()
    {
        // Arrange
        var query = new SearchRestaurantsQuery
        {
            SearchTerm = "test",
            CategoryId = "cat1",
            PageSize = 15,
            PageNumber = 2
        };

        _searchServiceMock
            .Setup(s =>
                s.SearchAsync(
                    It.IsAny<SearchRestaurantsQuery>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new SearchRestaurantsResult());

        // Act
        await _handler.ExecuteAsync(query);

        // Assert
        _searchServiceMock
            .Verify(
                s =>
                    s.SearchAsync(
                        It.Is<SearchRestaurantsQuery>(q =>
                            q.SearchTerm == query.SearchTerm &&
                            q.CategoryId == query.CategoryId &&
                            q.PageSize == query.PageSize &&
                            q.PageNumber == query.PageNumber
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }
}