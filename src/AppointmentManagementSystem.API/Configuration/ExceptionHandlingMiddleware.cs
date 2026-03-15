using System.Text.Json;
using AppointmentManagementSystem.Domain.Errors;
using AppointmentManagementSystem.Domain.Exceptions;

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
            ArgumentException argumentException => new
            {
                errorCode = "ValidationError",
                message = argumentException.Message
            },
            DomainException domainException => new
            {
                errorCode = domainException.Message,
                message = domainException.Message
            },
            _ => new
            {
                errorCode = "UnknownError",
                message = "An unexpected error occurred."
            }
        };

        context.Response.StatusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            DomainException domainException when domainException.Message == DomainErrors.AppointmentNotFound => StatusCodes.Status404NotFound,
            DomainException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}
