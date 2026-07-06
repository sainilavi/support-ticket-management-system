using SupportTicketManagementSystem.Domain.Entities;

namespace SupportTicketManagementSystem.Application.Interfaces.Repositories;

public interface ITicketRepository
{
    Task<IReadOnlyList<Ticket>> GetAllWithUsersAsync(CancellationToken cancellationToken = default);
    Task<Ticket?> GetByIdWithUsersAsync(int id, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
