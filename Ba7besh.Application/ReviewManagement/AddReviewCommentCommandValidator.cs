using FluentValidation;

namespace Ba7besh.Application.ReviewManagement;

public class AddReviewCommentCommandValidator : AbstractValidator<AddReviewCommentCommand>
{
    public AddReviewCommentCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment content cannot be empty")
            .MaximumLength(1000)
            .WithMessage("Comment content cannot exceed 1000 characters");
    }
}