
using System.Net;
using System.Text.Json;

namespace SmartInventory.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            if (context.RequestAborted.IsCancellationRequested)
                throw;

            _logger.LogError(
                ex,
                "An unexpected error occurred while processing {Path}",
                context.Request.Path
            );

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;

            context.Response.ContentType = "application/json";

            var response = new
            {
                statusCode = 500,
                message = "An unexpected server error occurred."
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response)
            );
        }
    }
}
