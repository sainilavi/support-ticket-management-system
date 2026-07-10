using SupportTicketManagementSystem.Application.Interfaces.Repositories;
using SupportTicketManagementSystem.Infrastructure.Data;

namespace SupportTicketManagementSystem.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
