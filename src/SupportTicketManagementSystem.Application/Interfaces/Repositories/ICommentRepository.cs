using SupportTicketManagementSystem.Domain.Entities;

namespace SupportTicketManagementSystem.Application.Interfaces.Repositories;

public interface ICommentRepository
{
    Task<IReadOnlyList<Comment>> GetByTicketIdWithUserAsync(int ticketId, CancellationToken cancellationToken = default);
    Task<Comment> AddAsync(Comment comment, CancellationToken cancellationToken = default);
    Task LoadUserAsync(Comment comment, CancellationToken cancellationToken = default);
}
