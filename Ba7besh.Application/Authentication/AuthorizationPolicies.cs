namespace Ba7besh.Application.Authentication;

public static class AuthorizationPolicies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string BusinessOwner = nameof(BusinessOwner);
    public const string CanManageBusiness = nameof(CanManageBusiness);
    public const string CanEditReview = nameof(CanEditReview);
    public const string BotService = nameof(BotService);
}