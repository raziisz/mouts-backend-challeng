using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

public sealed class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationExceptionMiddleware> _logger;

    public ValidationExceptionMiddleware(
        RequestDelegate next,
        ILogger<ValidationExceptionMiddleware> logger)
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
        catch (ValidationException exception)
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                "ValidationError",
                "Invalid input data",
                string.Join("; ", exception.Errors.Select(error => $"{error.PropertyName}: {error.ErrorMessage}")));
        }
        catch (UnauthorizedAccessException exception)
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "AuthenticationError",
                "Authentication failed",
                exception.Message);
        }
        catch (DomainException exception)
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status409Conflict,
                "BusinessRuleViolation",
                "Business rule violation",
                exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status404NotFound,
                "ResourceNotFound",
                "Resource not found",
                exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status409Conflict,
                "Conflict",
                "Operation cannot be completed",
                exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while processing the request.");

            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "InternalServerError",
                "Internal server error",
                "An unexpected error occurred while processing the request.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string type, string error, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new ApiErrorResponse
        {
            Type = type,
            Error = error,
            Detail = detail
        });
    }
}
