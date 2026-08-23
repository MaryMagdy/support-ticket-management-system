using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common;
using SupportTickets.Application.DTOs;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;
using SupportTickets.Infrastructure.Persistence;
using SupportTickets.Infrastructure.Services;
using Xunit;

namespace SupportTickets.UnitTests;

public class TicketServiceTests
{
    private static AppDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<(AppDbContext db, User customerA, User customerB, User agent)> SeedAsync(string dbName)
    {
        var db = CreateDb(dbName);
        var customerA = new User { Email = "a@x.com", FullName = "A", Role = UserRole.Customer };
        var customerB = new User { Email = "b@x.com", FullName = "B", Role = UserRole.Customer };
        var agent = new User { Email = "agent@x.com", FullName = "Agent", Role = UserRole.SupportAgent };
        db.Users.AddRange(customerA, customerB, agent);
        await db.SaveChangesAsync();
        return (db, customerA, customerB, agent);
    }

    [Fact]
    public async Task Customer_CannotAccessAnotherCustomersTicket_ByIdManipulation()
    {
        var (db, customerA, customerB, _) = await SeedAsync(nameof(Customer_CannotAccessAnotherCustomersTicket_ByIdManipulation));

        var ticket = new Ticket { Title = "T", Description = "D", CustomerId = customerB.Id, Status = TicketStatus.Open, Priority = TicketPriority.Low };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(customerA.Id, UserRole.Customer);
        var service = new TicketService(db, currentUser);

        Func<Task> act = () => service.GetByIdAsync(ticket.Id);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Customer_CanAccessOwnTicket()
    {
        var (db, customerA, _, _) = await SeedAsync(nameof(Customer_CanAccessOwnTicket));

        var ticket = new Ticket { Title = "T", Description = "D", CustomerId = customerA.Id, Status = TicketStatus.Open, Priority = TicketPriority.Low };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(customerA.Id, UserRole.Customer);
        var service = new TicketService(db, currentUser);

        var result = await service.GetByIdAsync(ticket.Id);

        result.Id.Should().Be(ticket.Id);
    }

    [Fact]
    public async Task Agent_CannotAccessTicketNotAssignedToThem()
    {
        var (db, customerA, _, agent) = await SeedAsync(nameof(Agent_CannotAccessTicketNotAssignedToThem));

        var ticket = new Ticket { Title = "T", Description = "D", CustomerId = customerA.Id, Status = TicketStatus.Open, Priority = TicketPriority.Low, AssignedAgentId = null };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(agent.Id, UserRole.SupportAgent);
        var service = new TicketService(db, currentUser);

        Func<Task> act = () => service.GetByIdAsync(ticket.Id);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Customer_CannotChangePriority()
    {
        var (db, customerA, _, _) = await SeedAsync(nameof(Customer_CannotChangePriority));

        var ticket = new Ticket { Title = "T", Description = "D", CustomerId = customerA.Id, Status = TicketStatus.Open, Priority = TicketPriority.Low };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(customerA.Id, UserRole.Customer);
        var service = new TicketService(db, currentUser);

        Func<Task> act = () => service.UpdateAsync(ticket.Id, new UpdateTicketRequest(null, null, null, TicketPriority.Critical, null, null));

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task InvalidStatusTransition_Throws_ValidationAppException()
    {
        var (db, customerA, _, agent) = await SeedAsync(nameof(InvalidStatusTransition_Throws_ValidationAppException));

        var ticket = new Ticket { Title = "T", Description = "D", CustomerId = customerA.Id, Status = TicketStatus.Open, Priority = TicketPriority.Low, AssignedAgentId = agent.Id };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(agent.Id, UserRole.SupportAgent);
        var service = new TicketService(db, currentUser);

        // Agent trying to jump straight to Resolved from Open (must go through InProgress)
        Func<Task> act = () => service.UpdateAsync(ticket.Id, new UpdateTicketRequest(null, null, TicketStatus.Resolved, null, null, null));

        await act.Should().ThrowAsync<ValidationAppException>();
    }

    [Fact]
    public async Task ValidStatusTransition_Succeeds_AndLogsActivity()
    {
        var (db, customerA, _, agent) = await SeedAsync(nameof(ValidStatusTransition_Succeeds_AndLogsActivity));

        var ticket = new Ticket { Title = "T", Description = "D", CustomerId = customerA.Id, Status = TicketStatus.Open, Priority = TicketPriority.Low, AssignedAgentId = agent.Id };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(agent.Id, UserRole.SupportAgent);
        var service = new TicketService(db, currentUser);

        var result = await service.UpdateAsync(ticket.Id, new UpdateTicketRequest(null, null, TicketStatus.InProgress, null, null, null));

        result.Status.Should().Be(TicketStatus.InProgress);
        (await db.ActivityLogs.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task OnlyAdmin_CanDeleteTicket()
    {
        var (db, customerA, _, agent) = await SeedAsync(nameof(OnlyAdmin_CanDeleteTicket));

        var ticket = new Ticket { Title = "T", Description = "D", CustomerId = customerA.Id, Status = TicketStatus.Open, Priority = TicketPriority.Low };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        var agentUser = new FakeCurrentUserService(agent.Id, UserRole.SupportAgent);
        var agentService = new TicketService(db, agentUser);

        Func<Task> act = () => agentService.DeleteAsync(ticket.Id);
        await act.Should().ThrowAsync<ForbiddenException>();

        var adminUser = new FakeCurrentUserService(999, UserRole.Admin);
        var adminService = new TicketService(db, adminUser);
        await adminService.DeleteAsync(ticket.Id);

        (await db.Tickets.FindAsync(ticket.Id)).Should().BeNull();
    }
}
