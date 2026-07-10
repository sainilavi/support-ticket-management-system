using Microsoft.EntityFrameworkCore;
using SupportTicketManagementSystem.Application.DTOs.Tickets;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;
using SupportTicketManagementSystem.Domain.Entities;
using SupportTicketManagementSystem.Domain.Enums;
using SupportTicketManagementSystem.Infrastructure.Data;

namespace SupportTicketManagementSystem.Infrastructure.Repositories;

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<(IReadOnlyList<Ticket> Items, int TotalCount)> SearchAsync(
        TicketQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var ticketsQuery = _context.Tickets
            .AsNoTracking()
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            ticketsQuery = ticketsQuery.Where(t =>
                t.Title.Contains(keyword) ||
                t.Description.Contains(keyword));
        }

        if (query.Status.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.Status == query.Status.Value);
        }

        var totalCount = await ticketsQuery.CountAsync(cancellationToken);

        var items = await ticketsQuery
            .OrderByDescending(t => t.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Ticket?> GetByIdWithUsersAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Tickets
            .AsNoTracking()
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<Ticket?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task LoadUsersAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        await _context.Entry(ticket).Reference(t => t.CreatedBy).LoadAsync(cancellationToken);

        if (ticket.AssignedToUserId.HasValue)
        {
            await _context.Entry(ticket).Reference(t => t.AssignedTo).LoadAsync(cancellationToken);
        }
    }

    public async Task<TicketStatus?> GetStatusAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => (TicketStatus?)t.Status)
            .FirstOrDefaultAsync(cancellationToken);
}
