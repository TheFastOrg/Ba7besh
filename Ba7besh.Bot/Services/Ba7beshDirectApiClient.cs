using Ba7besh.Application.BusinessDiscovery;
using Ba7besh.Application.ReviewManagement;
using Ba7besh.Bot.Data;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Ba7besh.Bot.Services;

// Alternative implementation that directly accesses the database
public class Ba7beshDirectApiClient(Ba7beshDbContext dbContext, ILogger<Ba7beshDirectApiClient> logger)
    : IBa7beshApiClient
{
    public async Task<SearchBusinessesResult> SearchBusinessesAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            var sql = @"
                WITH filtered_businesses AS (
                    SELECT b.*
                    FROM businesses b
                    WHERE b.is_deleted = FALSE
                    AND (b.ar_name ILIKE @SearchTerm OR b.en_name ILIKE @SearchTerm)
                ),
                paginated_businesses AS (
                    SELECT *
                    FROM filtered_businesses
                    ORDER BY created_at DESC
                    OFFSET 0 ROWS
                    FETCH NEXT 10 ROWS ONLY
                )
                SELECT 
                    b.id,
                    b.ar_name,
                    b.en_name,
                    b.city,
                    b.type,
                    ST_Y(b.location::geometry) as ""Latitude"",
                    ST_X(b.location::geometry) as ""Longitude"",
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
                LEFT JOIN business_working_hours wh ON b.id = wh.business_id AND wh.is_deleted = FALSE";

            var businessDictionary = new Dictionary<string, BusinessSummary>();
            var totalCount = 0;
            
            await dbContext.Connection.QueryAsync<
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
                new { SearchTerm = $"%{searchTerm}%" },
                splitOn: "Latitude, Longitude,category_id,tag,business_id,total_count");

            return new SearchBusinessesResult
            {
                Businesses = businessDictionary.Values.ToList(),
                TotalCount = totalCount,
                PageSize = 10,
                PageNumber = 1
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching businesses");
            throw;
        }
    }

    public async Task<bool> SubmitReviewAsync(SubmitReviewCommand review, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate a new review ID
            var reviewId = Guid.NewGuid().ToString("N");
            
            // Begin transaction
            using var transaction = dbContext.Connection.BeginTransaction();
            
            try
            {
                // Insert the review
                await dbContext.Connection.ExecuteAsync(
                    @"INSERT INTO reviews (id, business_id, user_id, overall_rating, content, status, created_at)
                      VALUES (@Id, @BusinessId, @UserId, @OverallRating, @Content, @Status, @CreatedAt)",
                    new
                    {
                        Id = reviewId,
                        review.BusinessId,
                        review.UserId,
                        review.OverallRating,
                        review.Content,
                        Status = "pending",
                        CreatedAt = DateTime.UtcNow
                    }, 
                    transaction);
                
                // Insert dimension ratings if any
                if (review.DimensionRatings?.Any() == true)
                {
                    foreach (var dimensionRating in review.DimensionRatings)
                    {
                        await dbContext.Connection.ExecuteAsync(
                            @"INSERT INTO review_ratings (review_id, dimension, rating, note)
                              VALUES (@ReviewId, @Dimension::review_dimension, @Rating, @Note)",
                            new
                            {
                                ReviewId = reviewId,
                                Dimension = dimensionRating.Dimension.ToString().ToLower(),
                                dimensionRating.Rating,
                                dimensionRating.Note
                            },
                            transaction);
                    }
                }
                
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting review");
            return false;
        }
    }

    public async Task<IReadOnlyList<BusinessSummaryWithStats>> GetTopRatedBusinessesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var sql = @"
                WITH business_ratings AS (
                    SELECT 
                        business_id,
                        AVG(overall_rating) as avg_rating,
                        COUNT(*) as review_count
                    FROM reviews
                    WHERE status = 'approved' AND is_deleted = false
                    GROUP BY business_id
                    HAVING AVG(overall_rating) >= 4
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
                LIMIT 5";

            var businessDictionary = new Dictionary<string, BusinessSummaryWithStats>();

            await dbContext.Connection
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
                    splitOn: "latitude,longitude,category_id,tag,day");

            return businessDictionary.Values.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting top rated businesses");
            throw;
        }
    }
}