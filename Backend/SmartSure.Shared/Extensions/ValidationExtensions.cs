using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace SmartSure.Shared.Extensions;

public static class ValidationExtensions
{
    /// <summary>
    /// Configures the API behaviour to return consistent ProblemDetails for validation errors
    /// instead of the default ASP.NET Core format.
    /// </summary>
    public static IServiceCollection AddSmartSureValidation(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = new List<string>();
                foreach (var kvp in context.ModelState)
                {
                    foreach (var error in kvp.Value!.Errors)
                    {
                        var message = string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? "Invalid value provided."
                            : error.ErrorMessage;
                        errors.Add(message);
                    }
                }

                var detail = string.Join(" ", errors);

                var problem = new
                {
                    type = "about:blank",
                    title = "Validation Error",
                    status = StatusCodes.Status400BadRequest,
                    detail,
                    traceId = context.HttpContext.TraceIdentifier
                };

                return new BadRequestObjectResult(problem)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });

        return services;
    }
}
