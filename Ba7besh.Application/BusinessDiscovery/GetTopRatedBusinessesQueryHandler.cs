using System.Data;
using Dapper;
using Paramore.Darker;

namespace Ba7besh.Application.BusinessDiscovery;

public class GetTopRatedBusinessesQueryHandler(IDbConnection db)
    : QueryHandlerAsync<GetTopRatedBusinessesQuery, IReadOnlyList<BusinessSummaryWithStats>>
{
    public override async Task<IReadOnlyList<BusinessSummaryWithStats>> ExecuteAsync(
        GetTopRatedBusinessesQuery query,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                           WITH business_ratings AS (
                               SELECT 
                                   business_id,
                                   AVG(overall_rating) as avg_rating,
                                   COUNT(*) as review_count
                               FROM reviews
                               WHERE status = 'approved' AND is_deleted = false
                               GROUP BY business_id
                               HAVING AVG(overall_rating) >= @MinimumRating
                           )
                           SELECT 
                               b.id,
                               b.ar_name,
                               b.en_name,
                               b.city,
                               b.type,
                               br.avg_rating as average_rating,
                               br.review_count,
                               ST_Y(b.location::geometry) as latitude,
                               ST_X(b.location::geometry) as longitude,
                               c.id as category_id,
                               c.ar_name,
                               c.en_name,
                               bt.tag,
                               wh.day,
                               wh.opening_time::text,
                               wh.closing_time::text
                           FROM businesses b
                           JOIN business_ratings br ON b.id = br.business_id
                           LEFT JOIN business_categories bc ON b.id = bc.business_id AND bc.is_deleted = false
                           LEFT JOIN categories c ON bc.category_id = c.id AND c.is_deleted = false
                           LEFT JOIN business_tags bt ON b.id = bt.business_id AND bt.is_deleted = false
                           LEFT JOIN business_working_hours wh ON b.id = wh.business_id AND wh.is_deleted = false
                           WHERE b.is_deleted = false
                           ORDER BY br.avg_rating DESC, br.review_count DESC
                           LIMIT @Limit
                           """;

        var businessDictionary = new Dictionary<string, BusinessSummaryWithStats>();

        await db
            .QueryAsync<BusinessSummaryWithStats, double, double, CategoryInfo, string, WorkingHours,
                BusinessSummaryWithStats>(
                sql,
                (business, latitude, longitude, category, tag, workingHour) =>
                {
                    if (!businessDictionary.TryGetValue(business.Id, out var existingBusiness))
                    {
                        business.Location = new Location { Latitude = latitude, Longitude = longitude };
                        businessDictionary[business.Id] = business;
                        existingBusiness = business;
                    }

                    if (existingBusiness.Categories.All(c => c.Id != category.Id))
                        existingBusiness.Categories.Add(category);

                    if (!string.IsNullOrEmpty(tag) && !existingBusiness.Tags.Contains(tag))
                        existingBusiness.Tags.Add(tag);

                    if (existingBusiness.WorkingHours.All(wh => wh.Day != workingHour.Day))
                        existingBusiness.WorkingHours.Add(workingHour);

                    return business;
                },
                new { query.MinimumRating, query.Limit },
                splitOn: "latitude,longitude,category_id,tag,day");

        return businessDictionary.Values.ToList();
    }
}