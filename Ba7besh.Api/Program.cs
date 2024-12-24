using Ba7besh.Application.Authentication;
using Ba7besh.Application.CategoryManagement;
using Ba7besh.Application.RestaurantDiscovery;
using Ba7besh.Infrastructure;
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
builder.Services.AddSingleton<IRestaurantSearchService>(_ => 
    new CsvRestaurantSearchService(
        Path.Combine("Data", "business.csv"),
        Path.Combine("Data", "category.csv"),
        Path.Combine("Data", "business_categories.csv"),
        Path.Combine("Data", "business_working_hours.csv"),
        Path.Combine("Data", "business_tags.csv")));
builder.Services.AddSingleton<ICategoryRepository>(_ => 
    new CsvCategoryRepository(Path.Combine("Data", "category.csv")));
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

app.UseHttpsRedirection();

app.MapControllers();
app.MapGet("/", () => "Hello World! v3");

app.Run();