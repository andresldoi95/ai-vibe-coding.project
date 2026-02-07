using FluentValidation;
using System.Net;

namespace SaaS.Api.Middleware;

/// <summary>
/// Global exception handler middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException validationException => new
            {
                statusCode = (int)HttpStatusCode.BadRequest,
                message = "Validation failed",
                errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    )
            },
            UnauthorizedAccessException => new
            {
                statusCode = (int)HttpStatusCode.Unauthorized,
                message = "Unauthorized access",
                errors = new Dictionary<string, string[]>()
            },
            KeyNotFoundException => new
            {
                statusCode = (int)HttpStatusCode.NotFound,
                message = "Resource not found",
                errors = new Dictionary<string, string[]>()
            },
            _ => new
            {
                statusCode = (int)HttpStatusCode.InternalServerError,
                message = "An internal server error occurred",
                errors = new Dictionary<string, string[]>()
            }
        };

        context.Response.StatusCode = response.statusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}
