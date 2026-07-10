using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.DTOs.Comments;

public class GetCommentsByTicketRequest
{
    public int TicketId { get; set; }
}
