using SupportTicketManagementSystem.Domain.Common;

namespace SupportTicketManagementSystem.Domain.Entities;

public class Comment : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public int TicketId { get; set; }
    public int UserId { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User User { get; set; } = null!;
}
