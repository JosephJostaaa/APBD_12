using System.Net;
using System.Text.Json;
using Task12.Exceptions;

namespace Task12.Middlewears;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        if(exception.GetType() == typeof(NotFoundException))
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        
        
        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "An unexpected error occurred.",
            Details = exception.Message
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(jsonResponse);
    }
}