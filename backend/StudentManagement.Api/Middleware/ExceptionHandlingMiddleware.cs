using System.Net;
using System.Text.Json;
using StudentManagement.Api.DTOs.Common;

namespace StudentManagement.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred during request execution: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            KeyNotFoundException knf => ((int)HttpStatusCode.NotFound, knf.Message),
            InvalidOperationException ioe => ((int)HttpStatusCode.BadRequest, ioe.Message),
            ArgumentException ae => ((int)HttpStatusCode.BadRequest, ae.Message),
            UnauthorizedAccessException uae => ((int)HttpStatusCode.Unauthorized, uae.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected server error occurred. Please try again later.")
        };

        context.Response.StatusCode = statusCode;

        var traceId = context.TraceIdentifier;
        var response = ApiResponseDto<object>.Fail(message, null, traceId);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
