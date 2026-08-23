using FluentAssertions;
using SupportTickets.Domain.Enums;
using SupportTickets.Domain.Rules;
using Xunit;

namespace SupportTickets.UnitTests;

public class TicketStatusRulesTests
{
    [Theory]
    [InlineData(TicketStatus.Open, TicketStatus.InProgress, UserRole.SupportAgent, true)]
    [InlineData(TicketStatus.Open, TicketStatus.InProgress, UserRole.Customer, false)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Resolved, UserRole.SupportAgent, true)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Resolved, UserRole.Customer, false)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Closed, UserRole.Customer, true)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Closed, UserRole.SupportAgent, false)]
    [InlineData(TicketStatus.Open, TicketStatus.Resolved, UserRole.SupportAgent, false)] // skipping a step
    [InlineData(TicketStatus.Closed, TicketStatus.Open, UserRole.SupportAgent, false)] // invalid backward jump
    [InlineData(TicketStatus.Closed, TicketStatus.Open, UserRole.Customer, false)]
    public void IsValidTransition_EnforcesStateMachineAndRoles(TicketStatus from, TicketStatus to, UserRole role, bool expected)
    {
        TicketStatusRules.IsValidTransition(from, to, role).Should().Be(expected);
    }

    [Theory]
    [InlineData(TicketStatus.Closed, TicketStatus.Open)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Open)]
    [InlineData(TicketStatus.Closed, TicketStatus.InProgress)]
    public void Admin_CanOverrideAnyTransition(TicketStatus from, TicketStatus to)
    {
        TicketStatusRules.IsValidTransition(from, to, UserRole.Admin).Should().BeTrue();
    }

    [Fact]
    public void SameStatus_IsAlwaysValid_NoOp()
    {
        TicketStatusRules.IsValidTransition(TicketStatus.Open, TicketStatus.Open, UserRole.Customer).Should().BeTrue();
    }
}
