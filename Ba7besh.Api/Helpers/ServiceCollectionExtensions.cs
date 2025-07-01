using System.Data;
using System.Diagnostics.CodeAnalysis;
using Ba7besh.Api.Authentication;
using Ba7besh.Application.Authentication;
using Ba7besh.Application.BusinessDiscovery;
using Ba7besh.Application.DeviceManagement;
using Ba7besh.Application.ReviewManagement;
using Ba7besh.Infrastructure;
using Dapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Npgsql;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;

namespace Ba7besh.Api.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBa7beshInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        var dbConnectionString = configuration["DbConnectionString"];
        
        services.AddHealthChecks()
            .AddNpgSql(dbConnectionString);
        services.Configure<PhotoStorageOptions>(configuration.GetSection("PhotoStorage"));
        services.AddSingleton<IPhotoStorageService, AzurePhotoStorageService>();
        services.AddSingleton<IAuthService>(_ => 
            new FirebaseAuthService(configuration["Firebase:CredentialsPath"]));
        services.AddSingleton<IDbConnection>(_ => new NpgsqlConnection(dbConnectionString));
        services.AddSingleton<SignatureValidationService>();
        
        return services;
    }

    public static IServiceCollection AddBa7beshAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = async (context) =>
                    {
                        var token = context.Token;
                        Console.WriteLine($"JWT OnMessageReceived called with token: {(string.IsNullOrEmpty(token) ? "NULL" : token.Substring(0, Math.Min(20, token.Length)) + "...")}");
                        Console.WriteLine($"OnMessageReceived - context.Token: {(context.Token ?? "NULL")}");
                        Console.WriteLine($"OnMessageReceived - Authorization header: {context.Request.Headers.Authorization.ToString()}");

                        // Manual token extraction if context.Token is null
                        if (string.IsNullOrEmpty(token))
                        {
                            var authHeader = context.Request.Headers.Authorization.ToString();
                            Console.WriteLine($"Trying manual extraction from: {authHeader}");
                
                            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                token = authHeader.Substring("Bearer ".Length).Trim();
                                Console.WriteLine($"Manually extracted token: {(string.IsNullOrEmpty(token) ? "NULL" : token.Substring(0, Math.Min(20, token.Length)) + "...")}");
                            }
                        }
            
                        if (string.IsNullOrEmpty(token))
                        {
                            Console.WriteLine("No token found - skipping authentication");
                            return;
                        }

                        try
                        {
                            var authService = context.HttpContext.RequestServices
                                .GetRequiredService<IAuthService>();
                            var user = await authService.VerifyTokenAsync(token);
                            context.HttpContext.SetAuthenticatedUser(user);
                            Console.WriteLine($"JWT authentication successful for user: {user.UserId}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"JWT authentication failed: {ex.Message}");

                            context.Fail("Invalid token.");
                        }
                    }
                };
            })
            .AddScheme<AuthenticationSchemeOptions, BotAuthenticationHandler>("BotAuth", null);
            

        services.AddAuthorizationBuilder()
            .AddPolicy(
                AuthorizationPolicies.AdminOnly,
                policy => policy.Requirements.Add(new RoleRequirement(UserRole.Admin))
            ).AddPolicy(
                AuthorizationPolicies.BotService,
                policy => policy.RequireRole("BotService")
            );
            
        return services;
    }

    public static IServiceCollection AddBa7beshCQRS(this IServiceCollection services)
    {
        services.AddBrighter(options =>
            {
                options.HandlerLifetime = ServiceLifetime.Scoped;
                options.CommandProcessorLifetime = ServiceLifetime.Scoped;
                options.MapperLifetime = ServiceLifetime.Singleton;
            })
            .AutoFromAssemblies(typeof(GoogleAuthCommandHandler).Assembly);

        services.AddDarker(options => options.QueryProcessorLifetime = ServiceLifetime.Scoped)
            .AddHandlersFromAssemblies(typeof(SearchBusinessesQueryHandler).Assembly)
            .AddJsonQueryLogging()
            .AddDefaultPolicies();
            
        return services;
    }
    
    public static IServiceCollection AddBa7beshApi(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SubmitReviewCommandValidator>();
        services.AddFluentValidationAutoValidation();
        
        services.AddApiVersioning(
                options => { options.ReportApiVersions = true; })
            .AddApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });

        services.AddControllers(options => { options.RespectBrowserAcceptHeader = true; });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        return services;
    }
}