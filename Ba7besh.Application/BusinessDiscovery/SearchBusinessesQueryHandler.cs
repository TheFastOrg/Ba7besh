using System.Data;
using Dapper;
using Paramore.Darker;
using Paramore.Darker.QueryLogging;

namespace Ba7besh.Application.BusinessDiscovery;

public class SearchBusinessesQueryHandler(IDbConnection db)
    : QueryHandlerAsync<SearchBusinessesQuery, SearchBusinessesResult>
{
    [QueryLogging(1)]
    public override async Task<SearchBusinessesResult> ExecuteAsync(
        SearchBusinessesQuery query,
        CancellationToken cancellationToken = default)
    {
        var whereClauses = new List<string> { "b.is_deleted = FALSE" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            whereClauses.Add("(b.ar_name ILIKE @SearchTerm OR b.en_name ILIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{query.SearchTerm}%");
        }

        if (!string.IsNullOrEmpty(query.CategoryId))
        {
            whereClauses.Add(
                "EXISTS (SELECT 1 FROM business_categories bc WHERE bc.business_id = b.id AND bc.category_id = @CategoryId AND bc.is_deleted = FALSE)");
            parameters.Add("CategoryId", query.CategoryId);
        }

        if (query.Tags is { Length: > 0 })
        {
            whereClauses.Add(
                "EXISTS (SELECT 1 FROM business_tags bt WHERE bt.business_id = b.id AND bt.tag = ANY(@Tags) AND bt.is_deleted = FALSE)");
            parameters.Add("Tags", query.Tags);
        }
        var distanceSelect = ", NULL AS distance_km";
        if (query is { CenterLocation: not null, RadiusKm: not null })
        {
            whereClauses.Add("""
                             ST_DWithin(
                             location::geography,
                             ST_MakePoint(@Longitude, @Latitude)::geography,
                             @RadiusMeters
                             )
                             """);

            parameters.Add("Latitude", query.CenterLocation.Latitude);
            parameters.Add("Longitude", query.CenterLocation.Longitude);
            parameters.Add("RadiusMeters", query.RadiusKm.Value * 1000);
            distanceSelect = """
            ,
            ST_Distance(
                location::geography,
                ST_MakePoint(@Longitude, @Latitude)::geography
            ) / 1000 as distance_km
            """;
        }

        var whereClause = string.Join(" AND ", whereClauses);
        var sql = $"""
                   WITH filtered_businesses AS (
                       SELECT b.*
                              {distanceSelect}
                       FROM businesses b
                       WHERE {whereClause}
                   ),
                   paginated_businesses AS (
                       SELECT *
                       FROM filtered_businesses
                       ORDER BY created_at DESC
                       OFFSET @Offset ROWS
                       FETCH NEXT @PageSize ROWS ONLY
                   )
                   SELECT 
                       b.id,
                       b.ar_name,
                       b.en_name,
                       b.city,
                       b.type,
                       ST_Y(b.location::geometry) as "Latitude",
                       ST_X(b.location::geometry) as "Longitude",
                       b.distance_km,
                       bc.category_id,
                       c.*,
                       bt.tag,
                       wh.business_id,
                       wh.day,
                       wh.opening_time::text as opening_time,
                       wh.closing_time::text as closing_time,
                       (SELECT COUNT(*)::int FROM filtered_businesses) as total_count
                   FROM paginated_businesses b
                   LEFT JOIN business_categories bc ON b.id = bc.business_id AND bc.is_deleted = FALSE
                   LEFT JOIN categories c ON bc.category_id = c.id AND c.is_deleted = FALSE
                   LEFT JOIN business_tags bt ON b.id = bt.business_id AND bt.is_deleted = FALSE
                   LEFT JOIN business_working_hours wh ON b.id = wh.business_id AND wh.is_deleted = FALSE
                   """;

        parameters.Add("Offset", (query.PageNumber - 1) * query.PageSize);
        parameters.Add("PageSize", query.PageSize);

        var businessDictionary = new Dictionary<string, BusinessSummary>();
        var totalCount = 0;
        Type[] types = [typeof(BusinessSummary),
            typeof(double),
            typeof(double),
            typeof(double?),
            typeof(CategoryInfo),
            typeof(string),
            typeof(WorkingHours),
            typeof(int)];
        await db.QueryAsync<BusinessSummary?>(sql, types, objects =>
            {
                if (objects is
                    not
                    [
                        BusinessSummary business,
                        double latitude,
                        double longitude,
                        double distanceKm,
                        CategoryInfo category,
                        string tag,
                        WorkingHours workingHour,
                        int count
                    ]) return null;
                totalCount = count;

                if (!businessDictionary.TryGetValue(business.Id, out var existingBusiness))
                {
                    businessDictionary[business.Id] = business;
                    existingBusiness = business;
                }
                existingBusiness.Location = new Location
                {
                    Latitude = latitude,
                    Longitude = longitude
                };
                existingBusiness.DistanceInKm = distanceKm;
                if (existingBusiness.Categories.All(c => c.Id != category.Id))
                {
                    existingBusiness.Categories.Add(category);
                }

                if (!string.IsNullOrEmpty(tag) &&
                    !existingBusiness.Tags.Contains(tag))
                {
                    existingBusiness.Tags.Add(tag);
                }

                if (existingBusiness.WorkingHours.All(wh => wh.Day != workingHour.Day))
                {
                    existingBusiness.WorkingHours.Add(workingHour);
                }

                return business;

            },
            parameters,
            splitOn: "Latitude, Longitude, distance_km,category_id,tag,business_id,total_count" );
        await db.QueryAsync<
            BusinessSummary,
            double,
            double,
            CategoryInfo,
            string,
            WorkingHours,
            int,
            BusinessSummary>(
            sql,
            (business, latitude, longitude, category, tag, workingHour, count) =>
            {
                totalCount = count;

                if (!businessDictionary.TryGetValue(business.Id, out var existingBusiness))
                {
                    businessDictionary[business.Id] = business;
                    existingBusiness = business;
                }
                existingBusiness.Location = new Location
                {
                    Latitude = latitude,
                    Longitude = longitude
                };

                if (existingBusiness.Categories.All(c => c.Id != category.Id))
                {
                    existingBusiness.Categories.Add(category);
                }

                if (!string.IsNullOrEmpty(tag) &&
                    !existingBusiness.Tags.Contains(tag))
                {
                    existingBusiness.Tags.Add(tag);
                }

                if (existingBusiness.WorkingHours.All(wh => wh.Day != workingHour.Day))
                {
                    existingBusiness.WorkingHours.Add(workingHour);
                }

                return business;
            },
            parameters,
            splitOn: "Latitude, Longitude,category_id,tag,business_id,total_count");

        return new SearchBusinessesResult
        {
            Businesses = businessDictionary.Values.ToList(),
            TotalCount = totalCount,
            PageSize = query.PageSize,
            PageNumber = query.PageNumber
        };
    }
}