using Microsoft.AspNetCore.Builder;
using SmartSure.Shared.Middleware;

namespace SmartSure.Shared.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
