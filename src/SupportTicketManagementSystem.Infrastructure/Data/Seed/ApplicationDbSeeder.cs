using Microsoft.EntityFrameworkCore;
using SupportTicketManagementSystem.Domain.Common;
using SupportTicketManagementSystem.Domain.Entities;

namespace SupportTicketManagementSystem.Infrastructure.Data.Seed;

public static class ApplicationDbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedUsersAsync(context, cancellationToken);
        await SeedTicketsAsync(context, cancellationToken);
        await SeedCommentsAsync(context, cancellationToken);
    }

    private static Task SeedUsersAsync(ApplicationDbContext context, CancellationToken cancellationToken) =>
        SeedEntitiesAsync(context, context.Users, "Users", SeedData.Users, cancellationToken);

    private static Task SeedTicketsAsync(ApplicationDbContext context, CancellationToken cancellationToken) =>
        SeedEntitiesAsync(context, context.Tickets, "Tickets", SeedData.Tickets, cancellationToken);

    private static Task SeedCommentsAsync(ApplicationDbContext context, CancellationToken cancellationToken) =>
        SeedEntitiesAsync(context, context.Comments, "Comments", SeedData.Comments, cancellationToken);

    private static async Task SeedEntitiesAsync<TEntity>(
        ApplicationDbContext context,
        DbSet<TEntity> dbSet,
        string tableName,
        IEnumerable<TEntity> seedEntities,
        CancellationToken cancellationToken) where TEntity : BaseEntity
    {
        var existingIds = await dbSet
            .AsNoTracking()
            .Select(entity => entity.Id)
            .ToListAsync(cancellationToken);

        var entitiesToAdd = seedEntities
            .Where(entity => !existingIds.Contains(entity.Id))
            .ToList();

        if (entitiesToAdd.Count == 0)
        {
            return;
        }

        if (context.Database.IsSqlServer())
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            await SetIdentityInsertAsync(context, tableName, enabled: true, cancellationToken);
            dbSet.AddRange(entitiesToAdd);
            await context.SaveChangesAsync(cancellationToken);
            await SetIdentityInsertAsync(context, tableName, enabled: false, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return;
        }

        dbSet.AddRange(entitiesToAdd);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static Task SetIdentityInsertAsync(
        ApplicationDbContext context,
        string tableName,
        bool enabled,
        CancellationToken cancellationToken)
    {
        var sql = (tableName, enabled) switch
        {
            ("Users", true) => "SET IDENTITY_INSERT [Users] ON",
            ("Users", false) => "SET IDENTITY_INSERT [Users] OFF",
            ("Tickets", true) => "SET IDENTITY_INSERT [Tickets] ON",
            ("Tickets", false) => "SET IDENTITY_INSERT [Tickets] OFF",
            ("Comments", true) => "SET IDENTITY_INSERT [Comments] ON",
            ("Comments", false) => "SET IDENTITY_INSERT [Comments] OFF",
            _ => throw new ArgumentOutOfRangeException(nameof(tableName), tableName, "Unsupported seed table.")
        };

        return context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
