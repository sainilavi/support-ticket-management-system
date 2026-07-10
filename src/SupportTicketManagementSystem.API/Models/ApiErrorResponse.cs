namespace SupportTicketManagementSystem.API.Models;

public class ApiErrorResponse
{
    public bool Success { get; init; } = false;
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public IDictionary<string, string[]>? Errors { get; set; }
    public string? TraceId { get; set; }
    public string? Details { get; set; }
}
