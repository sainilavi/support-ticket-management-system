using SupportTicketManagementSystem.Application.DTOs.Tickets;

namespace SupportTicketManagementSystem.Application.DTOs.Tickets;

public class UpdateTicketRequest
{
    public int TicketId { get; set; }
    public UpdateTicketDto Dto { get; set; } = new();
}
