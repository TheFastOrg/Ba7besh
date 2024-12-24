using System.Globalization;
using Ba7besh.Application.CategoryManagement;
using CsvHelper;
using CsvHelper.Configuration;

namespace Ba7besh.Infrastructure;

public class CsvCategoryRepository(string categoriesCsvPath) : ICategoryRepository
{
    private List<CategoryRecord>? _categories;

    public async Task<IReadOnlyList<CategoryTreeNode>> GetCategoryTreeAsync(CancellationToken cancellationToken)
    {
        await LoadDataIfNeeded();

        var rootCategories = _categories!
            .Where(c => !c.is_deleted && string.IsNullOrEmpty(c.parent_id))
            .Select(BuildCategoryTreeNode)
            .ToList();

        return rootCategories;
    }

    private CategoryTreeNode BuildCategoryTreeNode(CategoryRecord category)
    {
        var subCategories = _categories!
            .Where(c => !c.is_deleted && c.parent_id == category.id)
            .Select(BuildCategoryTreeNode)
            .ToList();

        return new CategoryTreeNode
        {
            Id = category.id,
            ArName = category.ar_name,
            EnName = category.en_name,
            Slug = category.slug,
            SubCategories = subCategories
        };
    }

    private async Task LoadDataIfNeeded()
    {
        if (_categories != null) return;

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(categoriesCsvPath);
        using var csv = new CsvReader(reader, config);
        _categories = csv.GetRecords<CategoryRecord>().ToList();
    }
}