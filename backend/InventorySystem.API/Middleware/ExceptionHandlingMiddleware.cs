using FluentValidation;
using InventorySystem.Application.Common.Exceptions;
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
        catch (NotFoundException exception)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status404NotFound,
                "Kayıt bulunamadı.",
                exception.Message);
        }
        catch (ConflictException exception)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status409Conflict,
                "İşlem tamamlanamadı.",
                exception.Message);
        }
        catch (BusinessRuleException exception)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status422UnprocessableEntity,
                "İş kuralı ihlali.",
                exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Kimlik doğrulama başarısız.",
                string.IsNullOrWhiteSpace(exception.Message) ? null : exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled exception occurred.");
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Beklenmeyen bir hata oluştu.");
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

    private static Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string? detail = null)
    {
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            });
    }
}
