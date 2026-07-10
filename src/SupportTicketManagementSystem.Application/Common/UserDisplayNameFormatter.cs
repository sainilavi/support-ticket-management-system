using SupportTicketManagementSystem.Domain.Entities;

namespace SupportTicketManagementSystem.Application.Common;

public static class UserDisplayNameFormatter
{
    public static string Format(User user) =>
        $"{user.FirstName} {user.LastName}".Trim();
}
