using System.Net;
using System.Text.Json;
using FluentValidation;
using SupportTickets.Application.Common;

namespace SupportTickets.Api.Middleware;

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
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message, errors) = ex switch
        {
            AppException appEx => (appEx.StatusCode, appEx.Message, (ex as ValidationAppException)?.Errors),
            ValidationException fv => (400, "Validation failed.", (IDictionary<string, string[]>?)fv.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
            _ => (500, "An unexpected error occurred.", (IDictionary<string, string[]>?)null)
        };

        if (statusCode == 500)
        {
            _logger.LogError(ex, "Unhandled exception occurred processing {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogWarning("Handled exception ({StatusCode}) on {Path}: {Message}", statusCode, context.Request.Path, message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var body = new
        {
            status = statusCode,
            message,
            errors,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseAppExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
