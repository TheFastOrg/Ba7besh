using FluentValidation;

namespace Ba7besh.Application.BusinessDiscovery;

public class SearchBusinessesQueryValidator : AbstractValidator<SearchBusinessesQuery>
{
    public SearchBusinessesQueryValidator()
    {
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize cannot exceed 100.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("SearchTerm cannot exceed 100 characters.");

        RuleFor(x => x)
            .Must(x =>
                (x.CenterLocation == null && x.RadiusKm == null) ||
                (x.CenterLocation != null && x.RadiusKm != null))
            .WithMessage("Both CenterLocation and RadiusKm must be provided together or neither.");
    }
}