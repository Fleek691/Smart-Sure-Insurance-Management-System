using Microsoft.AspNetCore.Builder;
using SmartSure.Shared.Middleware;

namespace SmartSure.Shared.Extensions;

/// <summary>
/// Extension methods for registering SmartSure middleware in the request pipeline.
/// </summary>
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
