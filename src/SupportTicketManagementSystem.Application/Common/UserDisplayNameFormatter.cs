using SupportTicketManagementSystem.Domain.Entities;

namespace SupportTicketManagementSystem.Application.Common;

public static class UserDisplayNameFormatter
{
    public static string Format(User? user) =>
        user is null ? string.Empty : $"{user.FirstName} {user.LastName}".Trim();
}
