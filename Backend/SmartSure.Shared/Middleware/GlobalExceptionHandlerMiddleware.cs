using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartSure.Shared.Exceptions;

namespace SmartSure.Shared.Middleware;

/// <summary>
/// Catches domain exceptions and maps them to RFC 7807 problem+json responses.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error [{CorrelationId}]: {Message}", context.TraceIdentifier, ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized [{CorrelationId}]: {Message}", context.TraceIdentifier, ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning("Forbidden [{CorrelationId}]: {Message}", context.TraceIdentifier, ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Not found [{CorrelationId}]: {Message}", context.TraceIdentifier, ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning("Conflict [{CorrelationId}]: {Message}", context.TraceIdentifier, ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning("Business rule violation [{CorrelationId}]: {Message}", context.TraceIdentifier, ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status422UnprocessableEntity, ex.Message);
        }
        catch (HttpServiceException ex)
        {
            _logger.LogWarning(ex, "Downstream service error [{CorrelationId}]", context.TraceIdentifier);
            await WriteProblemDetailsAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception [{CorrelationId}]", context.TraceIdentifier);
            await WriteProblemDetailsAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string message)
    {
        if (context.Response.HasStarted) return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = "about:blank",
            title = ResolveTitle(statusCode),
            status = statusCode,
            detail = message,
            traceId = context.TraceIdentifier
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
    }

    private static string ResolveTitle(int statusCode) => statusCode switch
    {
        400 => "Validation Error",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Business Rule Violation",
        _ => "An unexpected error occurred"
    };
}
