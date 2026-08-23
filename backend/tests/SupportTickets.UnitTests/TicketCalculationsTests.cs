using FluentAssertions;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;
using SupportTickets.Domain.Rules;
using Xunit;

namespace SupportTickets.UnitTests;

public class TicketCalculationsTests
{
    [Fact]
    public void TotalTimeMinutes_SumsAllEntries()
    {
        var entries = new List<TimeEntry>
        {
            new() { DurationMinutes = 30 },
            new() { DurationMinutes = 45 },
            new() { DurationMinutes = 15 }
        };

        TicketCalculations.TotalTimeMinutes(entries).Should().Be(90);
    }

    [Fact]
    public void TotalTimeMinutes_EmptyList_ReturnsZero()
    {
        TicketCalculations.TotalTimeMinutes(new List<TimeEntry>()).Should().Be(0);
    }

    [Fact]
    public void AverageResolutionTimeHours_NoResolvedTickets_ReturnsNull()
    {
        var tickets = new List<Ticket>
        {
            new() { Status = TicketStatus.Open, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Status = TicketStatus.InProgress, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        TicketCalculations.AverageResolutionTimeHours(tickets).Should().BeNull();
    }

    [Fact]
    public void AverageResolutionTimeHours_ComputesAverageAcrossResolvedAndClosed()
    {
        var baseTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var tickets = new List<Ticket>
        {
            new() { Status = TicketStatus.Resolved, CreatedAt = baseTime, UpdatedAt = baseTime.AddHours(10) },
            new() { Status = TicketStatus.Closed, CreatedAt = baseTime, UpdatedAt = baseTime.AddHours(20) },
            new() { Status = TicketStatus.Open, CreatedAt = baseTime, UpdatedAt = baseTime.AddHours(100) } // excluded
        };

        var result = TicketCalculations.AverageResolutionTimeHours(tickets);

        result.Should().NotBeNull();
        result!.Value.Should().BeApproximately(15.0, 0.001);
    }
}
