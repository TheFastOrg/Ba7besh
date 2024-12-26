using System.Data;
using Ba7besh.Application.Authentication;
using Ba7besh.Application.Exceptions;
using Ba7besh.Application.RestaurantDiscovery;
using Ba7besh.Application.ReviewManagement;
using Ba7besh.Infrastructure;
using Dapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using Npgsql;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton<IAuthService>(_ =>
{
    var firebaseCredentialsPath = builder.Configuration["Firebase:CredentialsPath"];
    return new FirebaseAuthService(firebaseCredentialsPath);
});
DefaultTypeMap.MatchNamesWithUnderscores = true;
builder.Services.AddSingleton<IDbConnection>(_ =>
{
    var dbConnectionString = builder.Configuration["DbConnectionString"];
    return new NpgsqlConnection(dbConnectionString);
});
builder.Services.AddBrighter(options =>
    {
        //we want to use scoped, so make sure everything understands that which needs to
        options.HandlerLifetime = ServiceLifetime.Scoped;
        options.CommandProcessorLifetime = ServiceLifetime.Scoped;
        options.MapperLifetime = ServiceLifetime.Singleton;
    })
    .AutoFromAssemblies(typeof(RegisterUserCommandHandler).Assembly);
builder.Services.AddDarker(options => options.QueryProcessorLifetime = ServiceLifetime.Scoped)
    .AddHandlersFromAssemblies(typeof(SearchRestaurantsQueryHandler).Assembly)
    .AddJsonQueryLogging()
    .AddDefaultPolicies();
builder.Services.AddValidatorsFromAssemblyContaining<SubmitReviewCommandValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddApiVersioning(
        options => { options.ReportApiVersions = true; })
    .AddApiExplorer(
        options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
builder.Services.AddControllers(options => { options.RespectBrowserAcceptHeader = true; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler(applicationBuilder => 
{
    applicationBuilder.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            BusinessNotFoundException ex => (StatusCode: StatusCodes.Status404NotFound, Message: ex.Message),
            ValidationException ex => (StatusCode: StatusCodes.Status400BadRequest, Message: string.Join("; ", ex.Errors)),
            _ => (StatusCode: StatusCodes.Status500InternalServerError, Message: "An error occurred")
        };
        
        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsJsonAsync(new { error = response.Message });
    });
});

app.MapControllers();
app.MapGet("/", () => "Hello World! v3");

app.Run();