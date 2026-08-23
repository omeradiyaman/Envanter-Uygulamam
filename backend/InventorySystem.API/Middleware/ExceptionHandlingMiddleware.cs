using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            await WriteValidationProblemAsync(context, exception);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled exception occurred.");
            await WriteProblemAsync(context);
        }
    }

    private static Task WriteValidationProblemAsync(
        HttpContext context,
        ValidationException exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var errors = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return context.Response.WriteAsJsonAsync(
            new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed."
            });
    }

    private static Task WriteProblemAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred."
            });
    }
}
