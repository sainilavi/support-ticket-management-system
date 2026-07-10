namespace SupportTicketManagementSystem.Domain.Enums;

public static class TicketStatusWorkflow
{
    private static readonly IReadOnlyDictionary<TicketStatus, TicketStatus[]> AllowedTransitions =
        new Dictionary<TicketStatus, TicketStatus[]>
        {
            [TicketStatus.Open] = [TicketStatus.InProgress, TicketStatus.Cancelled],
            [TicketStatus.InProgress] = [TicketStatus.Resolved, TicketStatus.Cancelled],
            [TicketStatus.Resolved] = [TicketStatus.Closed],
            [TicketStatus.Closed] = [],
            [TicketStatus.Cancelled] = []
        };

    public static bool CanTransition(TicketStatus currentStatus, TicketStatus newStatus) =>
        currentStatus == newStatus ||
        (AllowedTransitions.TryGetValue(currentStatus, out var allowed) && allowed.Contains(newStatus));

    public static IReadOnlyList<TicketStatus> GetAllowedTransitions(TicketStatus currentStatus) =>
        AllowedTransitions.TryGetValue(currentStatus, out var allowed)
            ? allowed
            : Array.Empty<TicketStatus>();
}
