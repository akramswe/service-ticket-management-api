using Microsoft.EntityFrameworkCore;
using ServiceTicketManagement.Api.Domain;

namespace ServiceTicketManagement.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();

        if (await db.Tickets.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        db.Tickets.AddRange(
            new Ticket
            {
                Title = "Payment export fails for large files",
                Description = "Investigate export failures reported when a payment file contains many records.",
                Priority = TicketPriority.High,
                Status = TicketStatus.InProgress,
                Assignee = "Support Team",
                CreatedAtUtc = now.AddDays(-2),
                UpdatedAtUtc = now.AddHours(-4)
            },
            new Ticket
            {
                Title = "Add audit details to ticket history",
                Description = "Extend ticket history so support users can see when important changes were made.",
                Priority = TicketPriority.Medium,
                Status = TicketStatus.New,
                CreatedAtUtc = now.AddDays(-1),
                UpdatedAtUtc = now.AddDays(-1)
            },
            new Ticket
            {
                Title = "Incorrect status shown after resolution",
                Description = "A resolved ticket can still appear in the open ticket view in one workflow.",
                Priority = TicketPriority.Critical,
                Status = TicketStatus.Resolved,
                Assignee = "Application Support",
                CreatedAtUtc = now.AddDays(-5),
                UpdatedAtUtc = now.AddDays(-1)
            });

        await db.SaveChangesAsync();
    }
}
