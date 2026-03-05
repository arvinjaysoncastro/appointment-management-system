using System.Text.Json;
using AppointmentManagementSystem.Application.Exceptions;

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
            BusinessException businessException => new
            {
                errorCode = businessException.ErrorCode,
                message = businessException.Message
            },
            _ => new
            {
                errorCode = "UnknownError",
                message = "An unexpected error occurred."
            }
        };

        context.Response.StatusCode = exception switch
        {
            BusinessException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}
