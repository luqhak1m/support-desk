using Microsoft.EntityFrameworkCore;
using SupportDesk.Domain;

namespace SupportDesk.Infrastructure;

public class SupportDeskDbContext : DbContext
{
    public SupportDeskDbContext(DbContextOptions<SupportDeskDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Agent> Agents { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // agent
        modelBuilder.Entity<Agent>().Property(a => a.FullName).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Agent>().Property(a => a.Email).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Agent>().HasIndex(a => a.Email).IsUnique();
        modelBuilder.Entity<Agent>().Property(a => a.Department).HasConversion<string>();

        // ticket
        modelBuilder.Entity<Ticket>().Property(t => t.Reference).IsRequired().HasMaxLength(20);
        modelBuilder.Entity<Ticket>().HasIndex(t => t.Reference).IsUnique();
        modelBuilder.Entity<Ticket>().Property(t => t.Title).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Ticket>().Property(t => t.CustomerName).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Ticket>().Property(t => t.CustomerEmail).IsRequired().HasMaxLength(200);

        // store enums as text
        modelBuilder.Entity<Ticket>().Property(t => t.Status).HasConversion<string>();
        modelBuilder.Entity<Ticket>().Property(t => t.Priority).HasConversion<string>();

        // one ticket has many comments
        modelBuilder.Entity<Ticket>()
            .HasMany(t => t.Comments)
            .WithOne()
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // one agent can be assigned many tickets
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.AssignedAgent)
            .WithMany()
            .HasForeignKey(t => t.AssignedAgentId)
            .OnDelete(DeleteBehavior.SetNull);

        // comment 
        modelBuilder.Entity<Comment>().Property(c => c.AuthorName).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Comment>().Property(c => c.Body).IsRequired().HasMaxLength(4000);
    }
}