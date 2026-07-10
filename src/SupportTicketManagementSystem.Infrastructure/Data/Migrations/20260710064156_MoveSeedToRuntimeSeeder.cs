using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportTicketManagementSystem.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class MoveSeedToRuntimeSeeder : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Seed data is applied at runtime by ApplicationDbSeeder to prevent duplicates.
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
