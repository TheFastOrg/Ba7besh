using System.Globalization;
using Ba7besh.Application.RestaurantDiscovery;
using CsvHelper;
using CsvHelper.Configuration;

namespace Ba7besh.Infrastructure;



public class CsvRestaurantSearchService : IRestaurantSearchService
{
    private readonly string _businessCsvPath;
    private readonly string _categoriesCsvPath;
    private readonly string _workingHoursCsvPath;
    private readonly string _tagsCsvPath;
    
    private List<BusinessRecord>? _businesses;
    private List<BusinessCategoryRecord>? _businessCategories;
    private List<WorkingHourRecord>? _workingHours;
    private List<BusinessTagRecord>? _businessTags;

    public CsvRestaurantSearchService(
        string businessCsvPath,
        string categoriesCsvPath,
        string workingHoursCsvPath,
        string tagsCsvPath)
    {
        _businessCsvPath = businessCsvPath;
        _categoriesCsvPath = categoriesCsvPath;
        _workingHoursCsvPath = workingHoursCsvPath;
        _tagsCsvPath = tagsCsvPath;
    }

    public async Task<SearchRestaurantsResult> SearchAsync(
        SearchRestaurantsQuery query, 
        CancellationToken cancellationToken)
    {
        await LoadDataIfNeeded(cancellationToken);
        
        var filteredBusinesses = _businesses!
            .Where(b => !b.is_deleted)
            .Where(b => string.IsNullOrEmpty(query.SearchTerm) || 
                       b.ar_name.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                       b.en_name.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.CategoryId))
        {
            var businessIdsInCategory = _businessCategories!
                .Where(bc => bc.category_id == query.CategoryId && !bc.is_deleted)
                .Select(bc => bc.business_id)
                .ToHashSet();

            filteredBusinesses = filteredBusinesses.Where(b => businessIdsInCategory.Contains(b.id));
        }

        var totalCount = filteredBusinesses.Count();

        var pagedBusinesses = filteredBusinesses
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize);

        var restaurants = pagedBusinesses.Select(b => new RestaurantSummary
        {
            Id = b.id,
            ArName = b.ar_name,
            EnName = b.en_name,
            Location = b.location,
            City = b.city,
            Type = b.type,
            WorkingHours = GetWorkingHours(b.id),
            Categories = GetCategories(b.id),
            Tags = GetTags(b.id)
        }).ToList();

        return new SearchRestaurantsResult
        {
            Restaurants = restaurants,
            TotalCount = totalCount,
            PageSize = query.PageSize,
            PageNumber = query.PageNumber
        };
    }

    private IReadOnlyList<WorkingHours> GetWorkingHours(string businessId)
    {
        return _workingHours!
            .Where(wh => wh.business_id == businessId && !wh.is_deleted)
            .Select(wh => new WorkingHours
            {
                DayOfWeek = wh.day,
                OpeningTime = wh.opening_time,
                ClosingTime = wh.closing_time
            })
            .ToList();
    }

    private IReadOnlyList<string> GetCategories(string businessId)
    {
        return _businessCategories!
            .Where(bc => bc.business_id == businessId && !bc.is_deleted)
            .Select(bc => bc.category_id)
            .ToList();
    }

    private IReadOnlyList<string> GetTags(string businessId)
    {
        return _businessTags!
            .Where(bt => bt.business_id == businessId && !bt.is_deleted)
            .Select(bt => bt.tag)
            .ToList();
    }

    private async Task LoadDataIfNeeded(CancellationToken cancellationToken)
    {
        if (_businesses != null) return;

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        // Load businesses
        using (var reader = new StreamReader(_businessCsvPath))
        using (var csv = new CsvReader(reader, config))
        {
            _businesses = csv.GetRecords<BusinessRecord>().ToList();
        }

        // Load categories
        using (var reader = new StreamReader(_categoriesCsvPath))
        using (var csv = new CsvReader(reader, config))
        {
            _businessCategories = csv.GetRecords<BusinessCategoryRecord>().ToList();
        }

        // Load working hours
        using (var reader = new StreamReader(_workingHoursCsvPath))
        using (var csv = new CsvReader(reader, config))
        {
            _workingHours = csv.GetRecords<WorkingHourRecord>().ToList();
        }

        // Load tags
        using (var reader = new StreamReader(_tagsCsvPath))
        using (var csv = new CsvReader(reader, config))
        {
            _businessTags = csv.GetRecords<BusinessTagRecord>().ToList();
        }
    }
}

// CSV Record classes remain the same as before
public class BusinessRecord
{
    public string id { get; set; } = string.Empty;
    public string location { get; set; } = string.Empty;
    public string? address_line1 { get; set; }
    public string? address_line2 { get; set; }
    public string ar_name { get; set; } = string.Empty;
    public string city { get; set; } = string.Empty;
    public string country { get; set; } = string.Empty;
    public string en_name { get; set; } = string.Empty;
    public string slug { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public string created_at { get; set; } = string.Empty;
    public string? updated_at { get; set; }
    public string? deleted_at { get; set; }
    public bool is_deleted { get; set; }
}

public class BusinessCategoryRecord
{
    public string id { get; set; } = string.Empty;
    public string business_id { get; set; } = string.Empty;
    public string category_id { get; set; } = string.Empty;
    public string created_at { get; set; } = string.Empty;
    public string? updated_at { get; set; }
    public string? deleted_at { get; set; }
    public bool is_deleted { get; set; }
}

public class WorkingHourRecord
{
    public string id { get; set; } = string.Empty;
    public int day { get; set; }
    public string opening_time { get; set; } = string.Empty;
    public string closing_time { get; set; } = string.Empty;
    public string business_id { get; set; } = string.Empty;
    public string created_at { get; set; } = string.Empty;
    public string? updated_at { get; set; }
    public string? deleted_at { get; set; }
    public bool is_deleted { get; set; }
}

public class BusinessTagRecord
{
    public string id { get; set; } = string.Empty;
    public string tag { get; set; } = string.Empty;
    public string business_id { get; set; } = string.Empty;
    public string created_at { get; set; } = string.Empty;
    public string? updated_at { get; set; }
    public string? deleted_at { get; set; }
    public bool is_deleted { get; set; }
}