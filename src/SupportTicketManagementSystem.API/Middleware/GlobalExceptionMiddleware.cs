using System.Text.Json;
using SupportTicketManagementSystem.API.Models;

namespace SupportTicketManagementSystem.API.Middleware;

public class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var mapping = ExceptionMapper.Map(exception, _environment.IsDevelopment());

        LogException(context, exception, mapping);

        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                "The response has already started; unable to write error response. TraceId: {TraceId}",
                context.TraceIdentifier);
            throw exception;
        }

        var response = new ApiErrorResponse
        {
            StatusCode = (int)mapping.StatusCode,
            Message = mapping.Message,
            Errors = mapping.Errors,
            TraceId = context.TraceIdentifier,
            Details = mapping.Details
        };

        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)mapping.StatusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private void LogException(HttpContext context, Exception exception, ExceptionMappingResult mapping)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? string.Empty;
        var traceId = context.TraceIdentifier;

        if (mapping.LogLevel == LogLevel.Error)
        {
            _logger.LogError(
                exception,
                "Unhandled exception {ExceptionType} returned {StatusCode} for {Method} {Path}. TraceId: {TraceId}",
                exception.GetType().Name,
                (int)mapping.StatusCode,
                method,
                path,
                traceId);
            return;
        }

        _logger.LogWarning(
            exception,
            "Handled exception {ExceptionType} returned {StatusCode} for {Method} {Path}. TraceId: {TraceId}",
            exception.GetType().Name,
            (int)mapping.StatusCode,
            method,
            path,
            traceId);
    }
}
