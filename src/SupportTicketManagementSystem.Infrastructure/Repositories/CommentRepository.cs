using Microsoft.EntityFrameworkCore;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;
using SupportTicketManagementSystem.Domain.Entities;
using SupportTicketManagementSystem.Domain.Enums;
using SupportTicketManagementSystem.Infrastructure.Data;

namespace SupportTicketManagementSystem.Infrastructure.Repositories;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Comment>> GetByTicketIdWithUserAsync(
        int ticketId,
        CancellationToken cancellationToken = default) =>
        await _context.Comments
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task LoadUserAsync(Comment comment, CancellationToken cancellationToken = default) =>
        await _context.Entry(comment).Reference(c => c.User).LoadAsync(cancellationToken);
}
