using System.Net;
using QApplication.Exceptions;

namespace QAPI.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An error occurred while processing your request.";

        if (exception is HttpStatusCodeException httpStatusCodeException)
        {
            statusCode = httpStatusCodeException.StatusCode;
            message = httpStatusCodeException.Message;
        }
        
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            StatusCode = (int)statusCode,
            Message = _env.IsDevelopment() || _env.EnvironmentName == "Docker"
                ? message
                : "An error occurred while processing your request.",
            Detail = _env.IsDevelopment() || _env.EnvironmentName == "Docker"
                ? exception.StackTrace
                : null,
            Type = _env.IsDevelopment() || _env.EnvironmentName == "Docker"
                ? exception.GetType().Name
                : null
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}