using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<Report> Reports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organizer)
            .WithMany(u => u.OrganizedEvents)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Event)
            .WithMany(e => e.Tickets)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Customer)
            .WithMany(u => u.Tickets)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Ticket)
            .WithMany(t => t.Payments)
            .HasForeignKey(p => p.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Customer)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Promotion>()
            .HasOne(p => p.Event)
            .WithMany(e => e.Promotions)
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Report>()
            .HasOne(r => r.Organizer)
            .WithMany()
            .HasForeignKey(r => r.OrganizerId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure soft delete global query filters
        modelBuilder.Entity<Event>()
            .HasQueryFilter(e => e.DeletedAt == null);

        modelBuilder.Entity<Ticket>()
            .HasQueryFilter(t => t.DeletedAt == null);

        modelBuilder.Entity<Payment>()
            .HasQueryFilter(p => p.DeletedAt == null);

        modelBuilder.Entity<Promotion>()
            .HasQueryFilter(p => p.DeletedAt == null);

        modelBuilder.Entity<Report>()
            .HasQueryFilter(r => r.DeletedAt == null);

        // Configure unique constraints
        modelBuilder.Entity<Promotion>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.QrCode)
            .IsUnique();
    }
}
