using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupportTicketManagementSystem.Infrastructure.Data;
using SupportTicketManagementSystem.Infrastructure.Data.Seed;

namespace SupportTicketManagementSystem.API.Extensions;

public static class HostDatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (environment.IsDevelopment())
        {
            await dbContext.Database.MigrateAsync();
        }
        else if (environment.IsEnvironment("Testing"))
        {
            await dbContext.Database.EnsureCreatedAsync();
        }

        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            await ApplicationDbSeeder.SeedAsync(dbContext);
        }
    }
}
