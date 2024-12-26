using System.Data;
using Ba7besh.Application.Exceptions;
using Dapper;
using Npgsql;
using Paramore.Brighter;

namespace Ba7besh.Application.ReviewManagement;

public class SubmitReviewCommandHandler(IDbConnection db) : RequestHandlerAsync<SubmitReviewCommand>
{
    public override async Task<SubmitReviewCommand> HandleAsync(
        SubmitReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        if (db.State != ConnectionState.Open)
            await ((NpgsqlConnection)db).OpenAsync(cancellationToken);
        
        await ValidateBusinessExists(command.BusinessId);

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
                    Dimension = Enum.GetName(r.Dimension)?.ToLower(),
                    r.Rating,
                    r.Note
                });
                await db.ExecuteAsync("""
                                      INSERT INTO review_ratings (review_id, dimension, rating, note)
                                      VALUES (@ReviewId, @Dimension::review_dimension, @Rating, @Note)
                                      """,
                    param, transaction);
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

    private async Task ValidateBusinessExists(string businessId)
    {
        var businessExists = await db.QuerySingleOrDefaultAsync<bool>(
            "SELECT EXISTS(SELECT 1 FROM businesses WHERE id = @Id AND is_deleted = FALSE)",
            new { Id = businessId });

        if (!businessExists)
            throw new BusinessNotFoundException(businessId);
    }
}