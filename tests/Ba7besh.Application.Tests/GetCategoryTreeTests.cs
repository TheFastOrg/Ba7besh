using Ba7besh.Application.CategoryManagement;
using Moq;

namespace Ba7besh.Application.Tests;


public class GetCategoryTreeTests
{
   private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
   private readonly GetCategoryTreeQueryHandler _handler;

   public GetCategoryTreeTests()
   {
       _categoryRepositoryMock = new Mock<ICategoryRepository>();
       _handler = new GetCategoryTreeQueryHandler(_categoryRepositoryMock.Object);
   }

   [Fact]
   public async Task Should_Return_Root_Categories_With_SubCategories()
   {
       // Arrange
       var expectedTree = new List<CategoryTreeNode>
       {
           new()
           {
               Id = "1",
               ArName = "طعام",
               EnName = "Food",
               Slug = "food",
               SubCategories = new List<CategoryTreeNode>
               {
                   new()
                   {
                       Id = "2",
                       ArName = "مشاوي",
                       EnName = "Grills",
                       Slug = "grills",
                       SubCategories = Array.Empty<CategoryTreeNode>()
                   }
               }
           }
       };

       _categoryRepositoryMock
           .Setup(r => r.GetCategoryTreeAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(expectedTree);

       // Act
       var result = await _handler.ExecuteAsync(new GetCategoryTreeQuery());

       // Assert
       var rootCategory = Assert.Single(result);
       Assert.Equal("طعام", rootCategory.ArName);
       Assert.Equal("Food", rootCategory.EnName);
       
       var subCategory = Assert.Single(rootCategory.SubCategories);
       Assert.Equal("مشاوي", subCategory.ArName);
       Assert.Equal("Grills", subCategory.EnName);
   }

   [Fact]
   public async Task Should_Handle_Empty_Category_List()
   {
       // Arrange
       _categoryRepositoryMock
           .Setup(r => r.GetCategoryTreeAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(Array.Empty<CategoryTreeNode>());

       // Act
       var result = await _handler.ExecuteAsync(new GetCategoryTreeQuery());

       // Assert
       Assert.Empty(result);
   }
   
   [Fact]
   public async Task Should_Return_Multiple_Root_Categories()
   {
       // Arrange
       var expectedTree = new List<CategoryTreeNode>
       {
           new()
           {
               Id = "1",
               ArName = "طعام",
               EnName = "Food",
               Slug = "food",
               SubCategories = Array.Empty<CategoryTreeNode>()
           },
           new()
           {
               Id = "2", 
               ArName = "مشروبات",
               EnName = "Drinks",
               Slug = "drinks",
               SubCategories = Array.Empty<CategoryTreeNode>()
           }
       };

       _categoryRepositoryMock
           .Setup(r => r.GetCategoryTreeAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(expectedTree);

       // Act
       var result = await _handler.ExecuteAsync(new GetCategoryTreeQuery());

       // Assert
       Assert.Equal(2, result.Count);
       Assert.Contains(result, c => c.EnName == "Food");
       Assert.Contains(result, c => c.EnName == "Drinks");
   }
}