using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Training.SkillDevelopment.Extensions;

/// <summary>
/// Returns RFC 7807 ProblemDetails JSON for unhandled exceptions.
/// Shared pattern across all RH-OptimERP microservices (see rh-optimerp-frontend Issue #2).
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment environment)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception: {ExceptionType} at {Path}",
            exception.GetType().Name,
            httpContext.Request.Path);

        var (status, title) = MapException(exception);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = $"https://httpstatuses.com/{status}",
            Instance = httpContext.Request.Path,
            Detail = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred."
        };

        if (_environment.IsDevelopment())
        {
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;
            problem.Extensions["exceptionType"] = exception.GetType().FullName;
        }

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static (int status, string title) MapException(Exception exception) => exception switch
    {
        ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
        NotImplementedException => (StatusCodes.Status501NotImplemented, "Not Implemented"),
        TimeoutException => (StatusCodes.Status504GatewayTimeout, "Gateway Timeout"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };
}
