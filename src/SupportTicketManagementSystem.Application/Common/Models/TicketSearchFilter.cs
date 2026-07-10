using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.Common.Models;

public class TicketSearchFilter
{
    public string? Keyword { get; set; }
    public TicketStatus? Status { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
