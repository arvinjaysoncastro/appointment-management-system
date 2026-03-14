using System.Text.Json;
using AppointmentManagementSystem.Domain.Exceptions;
using FluentValidation;

namespace AppointmentManagementSystem.API.Configuration;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException validationException => new
            {
                errorCode = "ValidationError",
                message = validationException.Message
            },
            DomainException domainException => new
            {
                errorCode = "DomainError",
                message = domainException.Message
            },
            KeyNotFoundException notFoundException => new
            {
                errorCode = "NotFound",
                message = notFoundException.Message
            },
            _ => new
            {
                errorCode = "UnknownError",
                message = "An unexpected error occurred."
            }
        };

        context.Response.StatusCode = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ValidationException => StatusCodes.Status400BadRequest,
            DomainException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}
