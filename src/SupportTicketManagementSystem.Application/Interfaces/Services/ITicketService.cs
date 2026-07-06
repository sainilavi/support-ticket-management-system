using SupportTicketManagementSystem.Application.DTOs.Tickets;

namespace SupportTicketManagementSystem.Application.Interfaces.Services;

public interface ITicketService : IService
{
    Task<IReadOnlyList<TicketDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TicketDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TicketDto> CreateAsync(CreateTicketDto dto, CancellationToken cancellationToken = default);
    Task<TicketDto> UpdateAsync(int id, UpdateTicketDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
