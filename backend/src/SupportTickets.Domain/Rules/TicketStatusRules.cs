using SupportTickets.Domain.Enums;

namespace SupportTickets.Domain.Rules;

/// <summary>
/// Encapsulates the ticket status state machine and role-based transition rules.
/// </summary>
public static class TicketStatusRules
{
    // Allowed forward transitions (excluding admin override)
    private static readonly Dictionary<TicketStatus, TicketStatus[]> AllowedTransitions = new()
    {
        [TicketStatus.Open] = new[] { TicketStatus.InProgress },
        [TicketStatus.InProgress] = new[] { TicketStatus.Resolved, TicketStatus.Open },
        [TicketStatus.Resolved] = new[] { TicketStatus.Closed, TicketStatus.InProgress },
        [TicketStatus.Closed] = Array.Empty<TicketStatus>()
    };

    /// <summary>
    /// Determines whether a status transition is valid for the given role.
    /// Admin can override and perform any transition (including backwards jumps).
    /// </summary>
    public static bool IsValidTransition(TicketStatus from, TicketStatus to, UserRole role)
    {
        if (from == to) return true;

        if (role == UserRole.Admin)
        {
            // Admin can do anything except a no-op reverse-jump validation is skipped (full override)
            return true;
        }

        if (!AllowedTransitions.TryGetValue(from, out var allowed) || !allowed.Contains(to))
        {
            return false;
        }

        // Role-specific rules for non-admins
        switch (to)
        {
            case TicketStatus.InProgress:
                // Agent moves Open -> InProgress
                return role == UserRole.SupportAgent;
            case TicketStatus.Resolved:
                // Agent moves InProgress -> Resolved
                return role == UserRole.SupportAgent;
            case TicketStatus.Closed:
                // Only Customer (or Admin, handled above) can close a Resolved ticket
                return role == UserRole.Customer && from == TicketStatus.Resolved;
            case TicketStatus.Open:
                // Reopening from InProgress -> Open: agent only
                return role == UserRole.SupportAgent && from == TicketStatus.InProgress;
            default:
                return false;
        }
    }
}
