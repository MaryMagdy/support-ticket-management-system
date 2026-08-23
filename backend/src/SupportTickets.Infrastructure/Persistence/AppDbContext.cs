using Microsoft.EntityFrameworkCore;
using SupportTickets.Domain.Entities;

namespace SupportTickets.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.Email).IsRequired().HasMaxLength(256);
            b.Property(u => u.FullName).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Ticket>(b =>
        {
            b.Property(t => t.Title).IsRequired().HasMaxLength(300);
            b.Property(t => t.Description).IsRequired().HasMaxLength(4000);
            b.Ignore(t => t.TotalTimeMinutes);

            b.HasOne(t => t.Customer)
                .WithMany(u => u.CustomerTickets)
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(t => t.AssignedAgent)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedAgentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comment>(b =>
        {
            b.Property(c => c.Text).IsRequired().HasMaxLength(2000);
            b.HasOne(c => c.Ticket).WithMany(t => t.Comments).HasForeignKey(c => c.TicketId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ActivityLog>(b =>
        {
            b.HasOne(a => a.Ticket).WithMany(t => t.ActivityLogs).HasForeignKey(a => a.TicketId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TimeEntry>(b =>
        {
            b.HasOne(te => te.Ticket).WithMany(t => t.TimeEntries).HasForeignKey(te => te.TicketId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(te => te.User).WithMany().HasForeignKey(te => te.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.Property(r => r.Token).IsRequired().HasMaxLength(500);
            b.HasOne(r => r.User).WithMany(u => u.RefreshTokens).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
