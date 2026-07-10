using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.DTOs.Tickets;

/// <summary>
/// Request payload for updating a support ticket.
/// </summary>
public class UpdateTicketDto
{
    /// <summary>Short summary of the issue (max 200 characters).</summary>
    /// <example>Cannot login to the portal</example>
    public string Title { get; set; } = string.Empty;

    /// <summary>Detailed description of the issue (max 4000 characters).</summary>
    /// <example>Credentials verified. Escalated to engineering.</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>Current ticket status.</summary>
    /// <example>InProgress</example>
    public TicketStatus Status { get; set; }

    /// <summary>Ticket priority level.</summary>
    /// <example>High</example>
    public TicketPriority Priority { get; set; }

    /// <summary>Optional identifier of the agent or admin assigned to the ticket.</summary>
    /// <example>2</example>
    public int? AssignedToUserId { get; set; }
}
