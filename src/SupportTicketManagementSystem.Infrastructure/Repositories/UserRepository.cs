using Microsoft.EntityFrameworkCore;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;
using SupportTicketManagementSystem.Domain.Entities;
using SupportTicketManagementSystem.Domain.Enums;
using SupportTicketManagementSystem.Infrastructure.Data;

namespace SupportTicketManagementSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(u => u.Id == id, cancellationToken);

    public async Task<bool> ExistsActiveAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(u => u.Id == id && u.IsActive, cancellationToken);

    public async Task<bool> ExistsActiveWithRoleAsync(
        int id,
        UserRole role,
        CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(
            u => u.Id == id && u.IsActive && u.Role == role,
            cancellationToken);

    public async Task<bool> ExistsActiveWithAnyRoleAsync(
        int id,
        CancellationToken cancellationToken,
        params UserRole[] roles) =>
        await _context.Users.AnyAsync(
            u => u.Id == id && u.IsActive && roles.Contains(u.Role),
            cancellationToken);
}
