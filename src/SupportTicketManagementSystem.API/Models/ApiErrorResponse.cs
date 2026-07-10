namespace SupportTicketManagementSystem.API.Models;

/// <summary>
/// Standard API error response returned by the global exception handler.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>Indicates whether the request succeeded.</summary>
    /// <example>false</example>
    public bool Success { get; init; } = false;

    /// <summary>HTTP status code.</summary>
    /// <example>400</example>
    public int StatusCode { get; set; }

    /// <summary>High-level error message.</summary>
    /// <example>One or more validation errors occurred.</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>Field-level validation errors keyed by property name.</summary>
    public IDictionary<string, string[]>? Errors { get; set; }

    /// <summary>Request trace identifier for diagnostics.</summary>
    public string? TraceId { get; set; }

    /// <summary>Detailed exception information (development only).</summary>
    public string? Details { get; set; }
}
