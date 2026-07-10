using System.Net;
using SupportTicketManagementSystem.Application.Exceptions;
using SupportTicketManagementSystem.Domain.Exceptions;

namespace SupportTicketManagementSystem.API.Middleware;

public static class ExceptionMapper
{
    private const string InternalServerErrorMessage = "An unexpected error occurred.";
    private const string UnauthorizedMessage = "Unauthorized access.";

    public static ExceptionMappingResult Map(Exception exception, bool isDevelopment)
    {
        return exception switch
        {
            ValidationException validationException => new ExceptionMappingResult(
                HttpStatusCode.BadRequest,
                LogLevel.Warning,
                validationException.Message,
                validationException.Errors),

            NotFoundException notFoundException => new ExceptionMappingResult(
                HttpStatusCode.NotFound,
                LogLevel.Warning,
                notFoundException.Message,
                null),

            DomainException domainException => new ExceptionMappingResult(
                HttpStatusCode.BadRequest,
                LogLevel.Warning,
                domainException.Message,
                null),

            ArgumentNullException argumentNullException => new ExceptionMappingResult(
                HttpStatusCode.BadRequest,
                LogLevel.Warning,
                argumentNullException.Message,
                null),

            ArgumentException argumentException => new ExceptionMappingResult(
                HttpStatusCode.BadRequest,
                LogLevel.Warning,
                argumentException.Message,
                null),

            UnauthorizedAccessException => new ExceptionMappingResult(
                HttpStatusCode.Unauthorized,
                LogLevel.Warning,
                UnauthorizedMessage,
                null),

            KeyNotFoundException keyNotFoundException => new ExceptionMappingResult(
                HttpStatusCode.NotFound,
                LogLevel.Warning,
                keyNotFoundException.Message,
                null),

            InvalidOperationException invalidOperationException => new ExceptionMappingResult(
                HttpStatusCode.BadRequest,
                LogLevel.Warning,
                invalidOperationException.Message,
                null),

            _ => new ExceptionMappingResult(
                HttpStatusCode.InternalServerError,
                LogLevel.Error,
                isDevelopment ? exception.Message : InternalServerErrorMessage,
                null,
                isDevelopment ? exception.ToString() : null)
        };
    }
}

public sealed record ExceptionMappingResult(
    HttpStatusCode StatusCode,
    LogLevel LogLevel,
    string Message,
    IDictionary<string, string[]>? Errors,
    string? Details = null);
