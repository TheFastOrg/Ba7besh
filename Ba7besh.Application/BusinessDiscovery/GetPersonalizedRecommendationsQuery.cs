using System.Data;
using Dapper;
using Paramore.Darker;
using Paramore.Darker.QueryLogging;

namespace Ba7besh.Application.BusinessDiscovery;

public record GetPersonalizedRecommendationsQuery(
    string UserId,
    Location? Location = null,
    int Limit = 10) : IQuery<IReadOnlyList<BusinessSummaryWithStats>>;

public class GetPersonalizedRecommendationsQueryHandler(IDbConnection db)
    : QueryHandlerAsync<GetPersonalizedRecommendationsQuery, IReadOnlyList<BusinessSummaryWithStats>>
{
    [QueryLogging(1)]
    public override async Task<IReadOnlyList<BusinessSummaryWithStats>> ExecuteAsync(
        GetPersonalizedRecommendationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var distanceSelect = ", NULL AS distance_km";
        var distanceOrderBy = "";
        var parameters = new DynamicParameters(new { query.UserId, query.Limit });

        if (query.Location is not null)
        {
            distanceSelect = """
                ,
                ST_Distance(
                    b.location::geography,
                    ST_MakePoint(@Longitude, @Latitude)::geography
                ) / 1000 as distance_km
                """;
            distanceOrderBy = ", distance_km ASC";
            parameters.Add("Latitude", query.Location.Latitude);
            parameters.Add("Longitude", query.Location.Longitude);
        }

        var sql = $"""
                   WITH user_categories AS (
                       SELECT DISTINCT bc.category_id
                       FROM reviews r
                       JOIN businesses b ON r.business_id = b.id
                       JOIN business_categories bc ON b.id = bc.business_id
                       WHERE r.user_id = @UserId
                         AND r.overall_rating >= 4
                         AND r.is_deleted = FALSE
                         AND bc.is_deleted = FALSE
                   ),
                   category_matches AS (
                       SELECT 
                           b.id as business_id,
                           COUNT(DISTINCT uc.category_id) as matching_categories
                       FROM businesses b
                       JOIN business_categories bc ON b.id = bc.business_id
                       JOIN user_categories uc ON bc.category_id = uc.category_id
                       WHERE b.is_deleted = FALSE
                       GROUP BY b.id
                   ),
                   business_ratings AS (
                       SELECT 
                           business_id,
                           AVG(overall_rating) as avg_rating,
                           COUNT(*) as review_count
                       FROM reviews
                       WHERE status = 'approved' 
                         AND is_deleted = FALSE
                       GROUP BY business_id
                       HAVING COUNT(*) >= 3
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
                       ST_X(b.location::geometry) as longitude
                       {distanceSelect},
                       c.id as category_id,
                       c.ar_name,
                       c.en_name,
                       bt.tag,
                       wh.day,
                       wh.opening_time::text,
                       wh.closing_time::text
                   FROM businesses b
                   JOIN business_ratings br ON b.id = br.business_id
                   JOIN category_matches cm ON b.id = cm.business_id
                   LEFT JOIN business_categories bc ON b.id = bc.business_id AND bc.is_deleted = FALSE
                   LEFT JOIN categories c ON bc.category_id = c.id AND c.is_deleted = FALSE
                   LEFT JOIN business_tags bt ON b.id = bt.business_id AND bt.is_deleted = FALSE
                   LEFT JOIN business_working_hours wh ON b.id = wh.business_id AND wh.is_deleted = FALSE
                   WHERE NOT EXISTS (
                       SELECT 1 
                       FROM reviews r 
                       WHERE r.business_id = b.id 
                         AND r.user_id = @UserId
                         AND r.is_deleted = FALSE
                   )
                   ORDER BY cm.matching_categories DESC, br.avg_rating DESC {distanceOrderBy}
                   LIMIT @Limit
                   """;

        var businessDictionary = new Dictionary<string, BusinessSummaryWithStats>();

        await db.QueryAsync<BusinessSummaryWithStats, double, double, double?, CategoryInfo, string, WorkingHours,
            BusinessSummaryWithStats>(
            sql,
            (business, latitude, longitude, distance, category, tag, workingHour) =>
            {
                if (!businessDictionary.TryGetValue(business.Id, out var existingBusiness))
                {
                    business.Location = new Location { Latitude = latitude, Longitude = longitude };
                    business.DistanceInKm = distance;
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
            parameters,
            splitOn: "latitude,longitude,distance_km,category_id,tag,day");

        return businessDictionary.Values.ToList();
    }
}