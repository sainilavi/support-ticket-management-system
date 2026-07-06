using SupportTicketManagementSystem.Domain.Common;
using SupportTicketManagementSystem.Domain.Enums;

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
}
