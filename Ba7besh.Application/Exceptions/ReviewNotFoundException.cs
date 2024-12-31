namespace Ba7besh.Application.Exceptions;

public class ReviewNotFoundException(string reviewId)
    : Exception($"Review with ID '{reviewId}' was not found")
{
    public string ReviewId { get; } = reviewId;
}