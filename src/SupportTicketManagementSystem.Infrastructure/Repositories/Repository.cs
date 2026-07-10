using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SupportTicketManagementSystem.Domain.Common;
using SupportTicketManagementSystem.Infrastructure.Data;

namespace SupportTicketManagementSystem.Infrastructure.Repositories;

public abstract class Repository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
}
