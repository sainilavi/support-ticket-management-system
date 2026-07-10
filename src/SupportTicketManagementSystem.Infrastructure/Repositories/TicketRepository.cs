using Microsoft.EntityFrameworkCore;
using SupportTicketManagementSystem.Application.Common.Models;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;
using SupportTicketManagementSystem.Domain.Entities;
using SupportTicketManagementSystem.Infrastructure.Data;

namespace SupportTicketManagementSystem.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly ApplicationDbContext _context;

    public TicketRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Ticket> Items, int TotalCount)> SearchAsync(
        TicketSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = query.Where(t =>
                t.Title.Contains(keyword) ||
                t.Description.Contains(keyword));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
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

    public async Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        await _context.Tickets.AddAsync(ticket, cancellationToken);
        return ticket;
    }

    public Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        _context.Tickets.Update(ticket);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        _context.Tickets.Remove(ticket);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Tickets.AnyAsync(t => t.Id == id, cancellationToken);
}
