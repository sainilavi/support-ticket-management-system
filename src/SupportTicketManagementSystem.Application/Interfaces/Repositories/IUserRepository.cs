using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsActiveAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveWithRoleAsync(int id, UserRole role, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveWithAnyRoleAsync(
        int id,
        IEnumerable<UserRole> roles,
        CancellationToken cancellationToken = default);
}
