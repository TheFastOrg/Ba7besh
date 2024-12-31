using System.Data;
using Ba7besh.Application.Exceptions;
using Ba7besh.Application.Helpers;
using Dapper;
using Npgsql;
using Paramore.Brighter;

namespace Ba7besh.Application.ReviewManagement;

public class SubmitReviewCommandHandler(IDbConnection db, IPhotoStorageService photoStorage) : RequestHandlerAsync<SubmitReviewCommand>
{
    public override async Task<SubmitReviewCommand> HandleAsync(
        SubmitReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        if (db.State != ConnectionState.Open)
            await ((NpgsqlConnection)db).OpenAsync(cancellationToken);
        
        await BusinessHelpers.ValidateBusinessExists(db, command.BusinessId);

        var reviewId = Guid.NewGuid();
        using var transaction = db.BeginTransaction();
        try
        {
            await db.ExecuteAsync("""
                                  INSERT INTO reviews (id, business_id, user_id, overall_rating, content, status, created_at)
                                  VALUES (@Id, @BusinessId, @UserId, @OverallRating, @Content, @Status, @CreatedAt)
                                  """,
                new
                {
                    Id = reviewId,
                    command.BusinessId,
                    command.UserId,
                    command.OverallRating,
                    command.Content,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                }, transaction);

            if (command.DimensionRatings.Any())
            {
                var param = command.DimensionRatings.Select(r => new
                {
                    ReviewId = reviewId,
                    Dimension = r.Dimension.ToLowerString(),
                    r.Rating,
                    r.Note
                });
                await db.ExecuteAsync("""
                                      INSERT INTO review_ratings (review_id, dimension, rating, note)
                                      VALUES (@ReviewId, @Dimension::review_dimension, @Rating, @Note)
                                      """,
                    param, transaction);
            }
            
            if (command.Photos.Any())
            {
                foreach (var photo in command.Photos)
                {
                    var photoUrl = await photoStorage.UploadPhotoAsync(
                        photo.FileName,
                        photo.OpenReadStream(),
                        photo.ContentType);

                    await db.ExecuteAsync("""
                                           INSERT INTO review_photos (
                                               id, review_id, photo_url, description, 
                                               mime_type, size_bytes, created_at
                                           )
                                           VALUES (
                                               @Id, @ReviewId, @PhotoUrl, @Description,
                                               @MimeType, @SizeBytes, @CreatedAt
                                           )
                                           """,
                        new
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            ReviewId = reviewId.ToString(),
                            PhotoUrl = photoUrl,
                            photo.Description,
                            MimeType = photo.ContentType,
                            SizeBytes = photo.Length,
                            CreatedAt = DateTime.UtcNow
                        }, transaction);
                }
            }

            transaction.Commit();
            return command;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}