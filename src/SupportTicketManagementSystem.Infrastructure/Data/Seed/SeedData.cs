using SupportTicketManagementSystem.Domain.Entities;
using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Infrastructure.Data.Seed;

public static class SeedData
{
    public static readonly DateTime SeedCreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime SeedUpdatedAt = new(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);

    public static IReadOnlyList<User> Users { get; } =
    [
        new User
        {
            Id = 1,
            FirstName = "System",
            LastName = "Admin",
            Email = "admin@support.com",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = SeedCreatedAt
        },
        new User
        {
            Id = 2,
            FirstName = "Support",
            LastName = "Agent",
            Email = "agent@support.com",
            Role = UserRole.Agent,
            IsActive = true,
            CreatedAt = SeedCreatedAt
        },
        new User
        {
            Id = 3,
            FirstName = "John",
            LastName = "Customer",
            Email = "customer@support.com",
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = SeedCreatedAt
        }
    ];

    public static IReadOnlyList<Ticket> Tickets { get; } =
    [
        new Ticket
        {
            Id = 1,
            Title = "Cannot login to the portal",
            Description = "User receives a 401 error when signing in with valid credentials.",
            Status = TicketStatus.Open,
            Priority = TicketPriority.High,
            CreatedByUserId = 3,
            AssignedToUserId = 2,
            CreatedAt = SeedCreatedAt
        },
        new Ticket
        {
            Id = 2,
            Title = "Printer not working",
            Description = "Office printer on level 2 is offline and shows a paper jam warning.",
            Status = TicketStatus.InProgress,
            Priority = TicketPriority.Medium,
            CreatedByUserId = 3,
            AssignedToUserId = 2,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt
        },
        new Ticket
        {
            Id = 3,
            Title = "Billing inquiry",
            Description = "Customer needs clarification on the latest invoice charges.",
            Status = TicketStatus.Resolved,
            Priority = TicketPriority.Low,
            CreatedByUserId = 3,
            AssignedToUserId = 2,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt
        }
    ];

    public static IReadOnlyList<Comment> Comments { get; } =
    [
        new Comment
        {
            Id = 1,
            TicketId = 1,
            UserId = 2,
            Content = "We are investigating the login issue and will update you shortly.",
            CreatedAt = SeedCreatedAt
        },
        new Comment
        {
            Id = 2,
            TicketId = 1,
            UserId = 3,
            Content = "Thank you. Please let me know once access is restored.",
            CreatedAt = SeedUpdatedAt
        },
        new Comment
        {
            Id = 3,
            TicketId = 2,
            UserId = 2,
            Content = "A technician has been dispatched to inspect the printer.",
            CreatedAt = SeedUpdatedAt
        }
    ];
}
