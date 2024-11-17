using Ba7besh.Application.UserRegistration;
using Ba7besh.Infrastructure;
using Paramore.Brighter.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton<IRegisterUserService>(_ =>
{
    var firebaseCredentialsPath = builder.Configuration["Firebase:CredentialsPath"];
    return new FirebaseAuthService(firebaseCredentialsPath);
});
builder.Services.AddBrighter(options =>
    {
        //we want to use scoped, so make sure everything understands that which needs to
        options.HandlerLifetime = ServiceLifetime.Scoped;
        options.CommandProcessorLifetime = ServiceLifetime.Scoped;
        options.MapperLifetime = ServiceLifetime.Singleton;
    })
    .AutoFromAssemblies(typeof(RegisterUserCommandHandler).Assembly);
builder.Services.AddApiVersioning(
        options =>
        {
            options.ReportApiVersions = true;
        })
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