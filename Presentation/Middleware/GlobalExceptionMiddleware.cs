using System.Net;
using System.Text.Json;

namespace Presentation.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
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
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            error = _env.IsDevelopment() ? exception.Message : "An error occurred. Please try again.",
            stackTrace = _env.IsDevelopment() ? exception.StackTrace : null
        };

        if (context.Request.Path.StartsWithSegments("/api"))
        {
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        context.Response.Redirect("/Home/Error");
    }
}
