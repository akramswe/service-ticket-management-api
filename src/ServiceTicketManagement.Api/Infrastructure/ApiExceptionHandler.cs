using Microsoft.AspNetCore.Diagnostics;

namespace ServiceTicketManagement.Api.Infrastructure;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            InvalidTicketStatusTransitionException => (
                StatusCodes.Status409Conflict,
                "Invalid ticket status transition",
                exception.Message),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Unexpected server error",
                "An unexpected error occurred while processing the request.")
        };

        logger.LogError(exception, "Request failed with status code {StatusCode}", statusCode);

        httpContext.Response.StatusCode = statusCode;
        await Results.Problem(
                statusCode: statusCode,
                title: title,
                detail: detail)
            .ExecuteAsync(httpContext);

        return true;
    }
}
