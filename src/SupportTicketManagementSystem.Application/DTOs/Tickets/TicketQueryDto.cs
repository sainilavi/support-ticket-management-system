using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.DTOs.Tickets;

public class TicketQueryDto
{
    public string? Keyword { get; set; }
    public TicketStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
