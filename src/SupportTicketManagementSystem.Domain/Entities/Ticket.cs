using SupportTicketManagementSystem.Domain.Common;
using SupportTicketManagementSystem.Domain.Enums;
using SupportTicketManagementSystem.Domain.Exceptions;

namespace SupportTicketManagementSystem.Domain.Entities;

public class Ticket : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public int CreatedByUserId { get; set; }
    public int? AssignedToUserId { get; set; }

    public User CreatedBy { get; set; } = null!;
    public User? AssignedTo { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public bool AcceptsComments() =>
        Status is not (TicketStatus.Closed or TicketStatus.Cancelled);

    public void EnsureValidStatusTransition(TicketStatus newStatus)
    {
        if (TicketStatusWorkflow.CanTransition(Status, newStatus))
        {
            return;
        }

        var allowedTransitions = TicketStatusWorkflow.GetAllowedTransitions(Status);
        var allowedMessage = allowedTransitions.Count == 0
            ? "none (terminal status)"
            : string.Join(", ", allowedTransitions);

        throw new DomainException(
            $"Cannot transition from '{Status}' to '{newStatus}'. Allowed transitions: {allowedMessage}.");
    }
}
