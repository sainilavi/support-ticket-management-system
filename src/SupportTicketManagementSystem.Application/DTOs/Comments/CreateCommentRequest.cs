using SupportTicketManagementSystem.Application.DTOs.Comments;

namespace SupportTicketManagementSystem.Application.DTOs.Comments;

public class CreateCommentRequest
{
    public int TicketId { get; set; }
    public CreateCommentDto Dto { get; set; } = new();
}
