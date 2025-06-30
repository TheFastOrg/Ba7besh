using System.Diagnostics.CodeAnalysis;
using Ba7besh.Api.Middlewares;
using Ba7besh.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace Ba7besh.Api.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class ApplicationBuilderExtensions 
{
    public static IApplicationBuilder UseDevEnvFeatures(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHttpsRedirection();
        }
        return app;
    }

    public static IApplicationBuilder UseBa7beshExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(applicationBuilder => 
        {
            applicationBuilder.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                context.Response.ContentType = "application/json";
        
                var response = exception switch
                {
                    BusinessNotFoundException ex => (StatusCode: StatusCodes.Status404NotFound, Message: ex.Message),
                    ReviewNotFoundException ex => (StatusCode: StatusCodes.Status404NotFound, Message: ex.Message),
                    ValidationException ex => (StatusCode: StatusCodes.Status400BadRequest, Message: string.Join("; ", ex.Errors)),
                    _ => (StatusCode: StatusCodes.Status500InternalServerError, Message: "An error occurred")
                };
        
                context.Response.StatusCode = response.StatusCode;
                await context.Response.WriteAsJsonAsync(new { error = response.Message });
            });
        });
        return app;
    }

    public static IApplicationBuilder UseBa7beshPipeline(this IApplicationBuilder app)
    {
        app.UseMiddleware<LoggingMiddleware>();
        app.UseMiddleware<DeviceValidationMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}