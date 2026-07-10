using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.DTOs.Tickets;

/// <summary>
/// Ticket details returned by the API.
/// </summary>
public class TicketDto
{
    /// <summary>Ticket identifier.</summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>Short summary of the issue.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Detailed description of the issue.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Current ticket status.</summary>
    public TicketStatus Status { get; set; }

    /// <summary>Ticket priority level.</summary>
    public TicketPriority Priority { get; set; }

    /// <summary>Identifier of the customer who created the ticket.</summary>
    public int CreatedByUserId { get; set; }

    /// <summary>Display name of the customer who created the ticket.</summary>
    public string CreatedByName { get; set; } = string.Empty;

    /// <summary>Identifier of the assigned agent or admin.</summary>
    public int? AssignedToUserId { get; set; }

    /// <summary>Display name of the assigned agent or admin.</summary>
    public string? AssignedToName { get; set; }

    /// <summary>UTC timestamp when the ticket was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp when the ticket was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }
}
