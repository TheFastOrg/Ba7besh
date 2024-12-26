using Ba7besh.Application.CategoryManagement;

namespace Ba7besh.Application.Tests;

public class GetCategoryTreeTests : DatabaseTestBase
{
    private readonly GetCategoryTreeQueryHandler _handler;

    public GetCategoryTreeTests(PostgresContainerFixture fixture) : base(fixture)
    {
        _handler = new GetCategoryTreeQueryHandler(Connection);
    }

    protected override async Task SeedTestData()
    {
        await Connection.ExecuteAsync(@"
            INSERT INTO categories (id, ar_name, en_name, slug, created_at, is_deleted) VALUES 
            ('food-id', 'طعام', 'Food', 'food', CURRENT_TIMESTAMP, false);
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO categories (id, ar_name, en_name, slug, parent_id, created_at, is_deleted) VALUES 
            ('grills-id', 'مشاوي', 'Grills', 'grills', 'food-id', CURRENT_TIMESTAMP, false),
            ('pizza-id', 'بيتزا', 'Pizza', 'pizza', 'food-id', CURRENT_TIMESTAMP, false),
            ('deleted-category', 'محذوف', 'Deleted', 'deleted', 'food-id', CURRENT_TIMESTAMP, true);
        ");

        await Connection.ExecuteAsync(@"
            INSERT INTO categories (id, ar_name, en_name, slug, parent_id, created_at, is_deleted) VALUES 
            ('sub-grills-id', 'مشاوي دجاج', 'Chicken Grills', 'chicken-grills', 'grills-id', CURRENT_TIMESTAMP, false);
        ");
    }

    [Fact]
    public async Task Should_Return_Root_Categories_With_SubCategories()
    {
        var result = await _handler.ExecuteAsync(new GetCategoryTreeQuery());

        var rootCategory = Assert.Single(result);
        Assert.Equal("طعام", rootCategory.ArName);
        Assert.Equal("Food", rootCategory.EnName);
        Assert.Equal("food", rootCategory.Slug);
        
        Assert.Equal(2, rootCategory.SubCategories.Count);
        Assert.Contains(rootCategory.SubCategories, c => c.EnName == "Grills");
        Assert.Contains(rootCategory.SubCategories, c => c.EnName == "Pizza");
    }

    [Fact]
    public async Task Should_Return_Multi_Level_Category_Tree()
    {
        var result = await _handler.ExecuteAsync(new GetCategoryTreeQuery());

        var rootCategory = Assert.Single(result);
        var grillsCategory = rootCategory.SubCategories.Single(c => c.EnName == "Grills");
        
        var subGrillsCategory = Assert.Single(grillsCategory.SubCategories);
        Assert.Equal("مشاوي دجاج", subGrillsCategory.ArName);
        Assert.Equal("Chicken Grills", subGrillsCategory.EnName);
    }

    [Fact]
    public async Task Should_Not_Return_Deleted_Categories()
    {
        var result = await _handler.ExecuteAsync(new GetCategoryTreeQuery());

        var rootCategory = Assert.Single(result);
        Assert.DoesNotContain(rootCategory.SubCategories, c => c.EnName == "Deleted");
    }

    [Fact]
    public async Task Should_Handle_Empty_Categories()
    {
        await Connection.ExecuteAsync("DELETE FROM categories");
        
        var result = await _handler.ExecuteAsync(new GetCategoryTreeQuery());
        
        Assert.Empty(result);
    }
}