using Paramore.Brighter;

namespace Ba7besh.Application.ReviewManagement;

public class ReactToReviewCommand(string reviewId, string userId, ReviewReaction reaction) : Command(Guid.NewGuid())
{
    public string ReviewId { get; } = reviewId;
    public string UserId { get; } = userId;
    public ReviewReaction Reaction { get; } = reaction;
}