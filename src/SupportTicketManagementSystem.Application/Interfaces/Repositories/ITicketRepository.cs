using SupportTicketManagementSystem.Application.Common.Models;
using SupportTicketManagementSystem.Domain.Entities;
using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.Interfaces.Repositories;

public interface ITicketRepository
{
    Task<(IReadOnlyList<Ticket> Items, int TotalCount)> SearchAsync(
        TicketSearchFilter filter,
        CancellationToken cancellationToken = default);
    Task<Ticket?> GetByIdWithUsersAsync(int id, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<TicketStatus?> GetStatusAsync(int id, CancellationToken cancellationToken = default);
}
