using Microsoft.EntityFrameworkCore;
using ServiceTicketManagement.Api.Domain;

namespace ServiceTicketManagement.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var ticket = modelBuilder.Entity<Ticket>();
        ticket.HasKey(x => x.Id);
        ticket.Property(x => x.Title).HasMaxLength(120).IsRequired();
        ticket.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        ticket.Property(x => x.Assignee).HasMaxLength(100);
        ticket.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
        ticket.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        ticket.HasIndex(x => x.Status);
        ticket.HasIndex(x => x.Priority);
        ticket.HasMany(x => x.Comments)
            .WithOne(x => x.Ticket)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        var comment = modelBuilder.Entity<TicketComment>();
        comment.HasKey(x => x.Id);
        comment.Property(x => x.Author).HasMaxLength(100).IsRequired();
        comment.Property(x => x.Message).HasMaxLength(1000).IsRequired();
    }
}
