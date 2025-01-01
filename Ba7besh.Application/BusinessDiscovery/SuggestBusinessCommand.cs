using FluentValidation;
using Paramore.Brighter;

namespace Ba7besh.Application.BusinessDiscovery;

public class SuggestBusinessCommand() : Command(Guid.NewGuid())
{
    public required string UserId { get; init; }
    public required string ArName { get; init; }
    public required string EnName { get; init; }
    public required Location Location { get; init; }
    public required string Description { get; init; }
}

public class SuggestBusinessCommandValidator : AbstractValidator<SuggestBusinessCommand>
{
    public SuggestBusinessCommandValidator()
    {
        RuleFor(x => x.ArName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.EnName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.Location)
            .NotNull()
            .Must(l => l.Latitude is >= -90 and <= 90)
            .WithMessage("Latitude must be between -90 and 90 degrees")
            .Must(l => l.Longitude is >= -180 and <= 180)
            .WithMessage("Longitude must be between -180 and 180 degrees");
    }
}