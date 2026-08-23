using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher)
    {
        await db.Database.MigrateAsync();

        if (await db.Users.AnyAsync())
        {
            return; // already seeded
        }

        var now = DateTime.UtcNow;

        var admin = new User { Email = "admin@supporttickets.local", FullName = "Alice Admin", Role = UserRole.Admin, PasswordHash = hasher.Hash("Admin123!"), CreatedAt = now };
        var agent1 = new User { Email = "agent1@supporttickets.local", FullName = "Aaron Agent", Role = UserRole.SupportAgent, PasswordHash = hasher.Hash("Agent123!"), CreatedAt = now };
        var agent2 = new User { Email = "agent2@supporttickets.local", FullName = "Amy Agent", Role = UserRole.SupportAgent, PasswordHash = hasher.Hash("Agent123!"), CreatedAt = now };
        var cust1 = new User { Email = "customer1@supporttickets.local", FullName = "Carl Customer", Role = UserRole.Customer, PasswordHash = hasher.Hash("Customer123!"), CreatedAt = now };
        var cust2 = new User { Email = "customer2@supporttickets.local", FullName = "Cara Customer", Role = UserRole.Customer, PasswordHash = hasher.Hash("Customer123!"), CreatedAt = now };
        var cust3 = new User { Email = "customer3@supporttickets.local", FullName = "Chris Customer", Role = UserRole.Customer, PasswordHash = hasher.Hash("Customer123!"), CreatedAt = now };

        db.Users.AddRange(admin, agent1, agent2, cust1, cust2, cust3);
        await db.SaveChangesAsync();

        var customers = new[] { cust1, cust2, cust3 };
        var agents = new[] { agent1, agent2 };

        var titles = new[]
        {
            "Cannot log in to my account",
            "Payment failed during checkout",
            "App crashes on startup",
            "Feature request: dark mode",
            "Password reset email not received",
            "Slow performance on dashboard",
            "Incorrect invoice amount",
            "Unable to upload attachments",
            "Data export missing records",
            "Mobile app push notifications not working"
        };

        var statuses = new[] { TicketStatus.Open, TicketStatus.InProgress, TicketStatus.Resolved, TicketStatus.Closed };
        var priorities = new[] { TicketPriority.Low, TicketPriority.Medium, TicketPriority.High, TicketPriority.Critical };

        var tickets = new List<Ticket>();
        for (int i = 0; i < titles.Length; i++)
        {
            var status = statuses[i % statuses.Length];
            var createdAt = now.AddDays(-(i + 1) * 2);
            var updatedAt = status is TicketStatus.Resolved or TicketStatus.Closed
                ? createdAt.AddHours(4 + i)
                : createdAt;

            var ticket = new Ticket
            {
                Title = titles[i],
                Description = $"Detailed description for issue: {titles[i]}. Reported by customer via web portal.",
                Status = status,
                Priority = priorities[i % priorities.Length],
                CustomerId = customers[i % customers.Length].Id,
                AssignedAgentId = status == TicketStatus.Open ? null : agents[i % agents.Length].Id,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };
            tickets.Add(ticket);
        }

        db.Tickets.AddRange(tickets);
        await db.SaveChangesAsync();

        foreach (var ticket in tickets)
        {
            db.Comments.Add(new Comment
            {
                TicketId = ticket.Id,
                UserId = ticket.CustomerId,
                Text = "Please look into this as soon as possible.",
                CreatedAt = ticket.CreatedAt.AddMinutes(10)
            });

            if (ticket.AssignedAgentId.HasValue)
            {
                db.Comments.Add(new Comment
                {
                    TicketId = ticket.Id,
                    UserId = ticket.AssignedAgentId.Value,
                    Text = "Thanks for reporting, we are looking into it.",
                    CreatedAt = ticket.CreatedAt.AddHours(1)
                });

                db.TimeEntries.Add(new TimeEntry
                {
                    TicketId = ticket.Id,
                    UserId = ticket.AssignedAgentId.Value,
                    WorkDate = ticket.CreatedAt.AddHours(2),
                    DurationMinutes = 30 + (ticket.Id * 5 % 60),
                    Description = "Initial investigation",
                    CreatedAt = ticket.CreatedAt.AddHours(2)
                });

                db.ActivityLogs.Add(new ActivityLog
                {
                    TicketId = ticket.Id,
                    UserId = ticket.AssignedAgentId.Value,
                    Type = ActivityType.AssignmentChange,
                    OldValue = null,
                    NewValue = ticket.AssignedAgentId.Value.ToString(),
                    CreatedAt = ticket.CreatedAt.AddMinutes(30)
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
