using System.Text.Json;
using System.Text.Json.Serialization;
using SupportTicketManagementSystem.Application.Common.Models;
using SupportTicketManagementSystem.Application.DTOs.Comments;
using SupportTicketManagementSystem.Application.DTOs.Tickets;
using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.API.Swagger;

public static class SwaggerExamples
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true
    };

    public static readonly TicketDto SampleTicket = new()
    {
        Id = 1,
        Title = "Cannot login to the portal",
        Description = "User receives a 401 error when signing in with valid credentials.",
        Status = TicketStatus.Open,
        Priority = TicketPriority.High,
        CreatedByUserId = 3,
        CreatedByName = "John Customer",
        AssignedToUserId = 2,
        AssignedToName = "Support Agent",
        CreatedAt = new DateTime(2026, 7, 10, 9, 0, 0, DateTimeKind.Utc),
        UpdatedAt = null
    };

    public static readonly CommentDto SampleComment = new()
    {
        Id = 1,
        Content = "We are investigating this issue and will update you shortly.",
        TicketId = 1,
        UserId = 2,
        UserName = "Support Agent",
        CreatedAt = new DateTime(2026, 7, 10, 10, 30, 0, DateTimeKind.Utc),
        UpdatedAt = null
    };

    public static readonly CreateTicketDto SampleCreateTicketRequest = new()
    {
        Title = "Cannot login to the portal",
        Description = "User receives a 401 error when signing in with valid credentials.",
        Priority = TicketPriority.High,
        CreatedByUserId = 3,
        AssignedToUserId = 2
    };

    public static readonly UpdateTicketDto SampleUpdateTicketRequest = new()
    {
        Title = "Cannot login to the portal",
        Description = "Credentials verified. Escalated to engineering.",
        Status = TicketStatus.InProgress,
        Priority = TicketPriority.High,
        AssignedToUserId = 2
    };

    public static readonly CreateCommentDto SampleCreateCommentRequest = new()
    {
        Content = "We are investigating this issue and will update you shortly.",
        UserId = 2
    };

    public static readonly TicketQueryDto SampleTicketQuery = new()
    {
        Keyword = "login",
        Status = TicketStatus.Open,
        PageNumber = 1,
        PageSize = 10
    };

    public static readonly Models.ApiErrorResponse SampleValidationError = new()
    {
        StatusCode = 400,
        Message = "One or more validation errors occurred.",
        Errors = new Dictionary<string, string[]>
        {
            ["title"] = ["Title is required."],
            ["status"] = ["Cannot transition from 'Open' to 'Resolved'. Allowed transitions: InProgress, Cancelled."]
        },
        TraceId = "0HN7EXAMPLETRACEID"
    };

    public static readonly Models.ApiErrorResponse SampleNotFoundError = new()
    {
        StatusCode = 404,
        Message = "Ticket with key '99' was not found.",
        TraceId = "0HN7EXAMPLETRACEID"
    };

    public static string ToJson<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    public static string TicketSuccessResponse(TicketDto ticket, string? message = null) =>
        ToJson(ApiResponse<TicketDto>.Ok(ticket, message));

    public static string CommentSuccessResponse(CommentDto comment, string? message = null) =>
        ToJson(ApiResponse<CommentDto>.Ok(comment, message));

    public static string PagedTicketsSuccessResponse() =>
        ToJson(ApiResponse<PagedResult<TicketDto>>.Ok(PagedResult<TicketDto>.Create(
            [SampleTicket],
            totalCount: 1,
            pageNumber: 1,
            pageSize: 10)));

    public static string CommentsListSuccessResponse() =>
        ToJson(ApiResponse<IReadOnlyList<CommentDto>>.Ok([SampleComment]));

    public static string DeleteSuccessResponse() =>
        ToJson(new
        {
            success = true,
            message = "Ticket deleted successfully.",
            data = new { },
            errors = (string[]?)null
        });
}
