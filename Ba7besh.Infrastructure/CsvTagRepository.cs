using System.Globalization;
using Ba7besh.Application.TagManagement;
using CsvHelper;
using CsvHelper.Configuration;

namespace Ba7besh.Infrastructure;

public class CsvTagRepository(string tagsFilePath) : ITagRepository
{
    private List<string>? _tags;

    public async Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken cancellationToken)
    {
        await LoadDataIfNeeded();
        return _tags!;
    }

    private async Task LoadDataIfNeeded()
    {
        if (_tags != null) return;
        
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(tagsFilePath);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<BusinessTagRecord>();
        _tags = records
            .Where(t => !t.is_deleted)
            .Select(t => t.tag)
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }

    private class BusinessTagRecord
    {
        public string tag { get; set; } = string.Empty;
        public bool is_deleted { get; set; }
    }
}