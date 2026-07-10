using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.DTOs.Tickets;

/// <summary>
/// Request payload for creating a support ticket.
/// </summary>
public class CreateTicketDto
{
    /// <summary>Short summary of the issue (max 200 characters).</summary>
    /// <example>Cannot login to the portal</example>
    public string Title { get; set; } = string.Empty;

    /// <summary>Detailed description of the issue (max 4000 characters).</summary>
    /// <example>User receives a 401 error when signing in with valid credentials.</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>Ticket priority level.</summary>
    /// <example>High</example>
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    /// <summary>Identifier of the customer creating the ticket.</summary>
    /// <example>3</example>
    public int CreatedByUserId { get; set; }

    /// <summary>Optional identifier of the agent or admin assigned to the ticket.</summary>
    /// <example>2</example>
    public int? AssignedToUserId { get; set; }
}
