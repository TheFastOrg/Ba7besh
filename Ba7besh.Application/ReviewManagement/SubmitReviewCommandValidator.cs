using FluentValidation;

namespace Ba7besh.Application.ReviewManagement;

public class SubmitReviewCommandValidator : AbstractValidator<SubmitReviewCommand>
{
    public SubmitReviewCommandValidator()
    {
        RuleFor(x => x.OverallRating)
            .InclusiveBetween(1, 5)
            .WithMessage("Overall rating must be between 1 and 5");

        RuleFor(x => x.Content)
            .MaximumLength(2000)
            .When(x => x.Content != null)
            .WithMessage("Review content cannot exceed 2000 characters");

        RuleFor(x => x.DimensionRatings)
            .Must(ratings => ratings.All(r => r.Rating is >= 1 and <= 5))
            .WithMessage("All dimension ratings must be between 1 and 5")
            .When(x => x.DimensionRatings.Any());

        RuleFor(x => x.DimensionRatings)
            .Must(ratings => ratings.Select(r => r.Dimension).Distinct().Count() == ratings.Count)
            .WithMessage("Duplicate dimensions are not allowed")
            .When(x => x.DimensionRatings.Any());
    }
}