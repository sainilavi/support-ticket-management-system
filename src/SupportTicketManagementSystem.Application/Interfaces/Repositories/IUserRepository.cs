using SupportTicketManagementSystem.Domain.Entities;
using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveWithRoleAsync(int id, UserRole role, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveWithAnyRoleAsync(int id, CancellationToken cancellationToken, params UserRole[] roles);
}
