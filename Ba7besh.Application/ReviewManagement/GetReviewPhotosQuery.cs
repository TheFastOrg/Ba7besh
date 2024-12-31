using System.Data;
using Ba7besh.Application.Helpers;
using Dapper;
using Paramore.Darker;
using Paramore.Darker.QueryLogging;

namespace Ba7besh.Application.ReviewManagement;

public record GetReviewPhotosQuery(string ReviewId) : IQuery<IReadOnlyList<ReviewPhoto>>;

public class GetReviewPhotosQueryHandler(IDbConnection db) 
    : QueryHandlerAsync<GetReviewPhotosQuery, IReadOnlyList<ReviewPhoto>>
{
    [QueryLogging(1)]
    public override async Task<IReadOnlyList<ReviewPhoto>> ExecuteAsync(
        GetReviewPhotosQuery query,
        CancellationToken cancellationToken = default)
    {
        await ReviewHelpers.ValidateReviewExists(db, query.ReviewId);
        
        const string sql = """
                           SELECT 
                               id,
                               review_id,
                               photo_url,
                               description,
                               mime_type,
                               size_bytes
                           FROM review_photos
                           WHERE review_id = @ReviewId
                             AND is_deleted = FALSE
                           ORDER BY created_at DESC;
                           """;

        var photos = await db.QueryAsync<ReviewPhoto>(
            sql,
            new { query.ReviewId });

        return photos.ToList();
    }
}