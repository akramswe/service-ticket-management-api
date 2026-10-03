using Microsoft.EntityFrameworkCore;
using ServiceTicketManagement.Api.Data;
using ServiceTicketManagement.Api.Domain;
using ServiceTicketManagement.Api.DTOs;
using ServiceTicketManagement.Api.Infrastructure;
using ServiceTicketManagement.Api.Services;
using Xunit;

namespace ServiceTicketManagement.Tests;

public sealed class TicketServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesNewTicketWithExpectedValues()
    {
        await using var db = CreateDbContext();
        var service = new TicketService(db);

        var result = await service.CreateAsync(
            new CreateTicketRequest
            {
                Title = "  Login error  ",
                Description = "  Customer cannot log in to the portal.  ",
                Priority = TicketPriority.High,
                Assignee = "  Backend Team  "
            },
            CancellationToken.None);

        Assert.Equal("Login error", result.Title);
        Assert.Equal("Customer cannot log in to the portal.", result.Description);
        Assert.Equal(TicketPriority.High, result.Priority);
        Assert.Equal(TicketStatus.New, result.Status);
        Assert.Equal("Backend Team", result.Assignee);
        Assert.Equal(1, await db.Tickets.CountAsync());
    }

    [Fact]
    public async Task ChangeStatusAsync_AllowsValidWorkflowTransition()
    {
        await using var db = CreateDbContext();
        var ticket = AddTicket(db, TicketStatus.New);
        await db.SaveChangesAsync();
        var service = new TicketService(db);

        var result = await service.ChangeStatusAsync(
            ticket.Id,
            new UpdateTicketStatusRequest { Status = TicketStatus.InProgress },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(TicketStatus.InProgress, result.Status);
    }

    [Fact]
    public async Task ChangeStatusAsync_RejectsTransitionFromClosedTicket()
    {
        await using var db = CreateDbContext();
        var ticket = AddTicket(db, TicketStatus.Closed);
        await db.SaveChangesAsync();
        var service = new TicketService(db);

        await Assert.ThrowsAsync<InvalidTicketStatusTransitionException>(() =>
            service.ChangeStatusAsync(
                ticket.Id,
                new UpdateTicketStatusRequest { Status = TicketStatus.InProgress },
                CancellationToken.None));
    }

    [Fact]
    public async Task GetAllAsync_FiltersByPriority()
    {
        await using var db = CreateDbContext();
        AddTicket(db, TicketStatus.New, TicketPriority.Low, "Low priority ticket");
        AddTicket(db, TicketStatus.New, TicketPriority.Critical, "Critical ticket");
        await db.SaveChangesAsync();
        var service = new TicketService(db);

        var result = await service.GetAllAsync(
            status: null,
            priority: TicketPriority.Critical,
            assignee: null,
            search: null,
            CancellationToken.None);

        var ticket = Assert.Single(result);
        Assert.Equal("Critical ticket", ticket.Title);
        Assert.Equal(TicketPriority.Critical, ticket.Priority);
    }

    [Fact]
    public async Task AddCommentAsync_AddsCommentAndUpdatesTicket()
    {
        await using var db = CreateDbContext();
        var ticket = AddTicket(db, TicketStatus.InProgress);
        await db.SaveChangesAsync();
        var beforeUpdate = ticket.UpdatedAtUtc;
        var service = new TicketService(db);

        var comment = await service.AddCommentAsync(
            ticket.Id,
            new AddCommentRequest
            {
                Author = "Washim Akram",
                Message = "Issue reproduced and logs reviewed."
            },
            CancellationToken.None);

        Assert.NotNull(comment);
        Assert.Equal("Washim Akram", comment.Author);
        Assert.Equal(1, await db.TicketComments.CountAsync());
        Assert.True(ticket.UpdatedAtUtc >= beforeUpdate);
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStatusAndPriorityCounts()
    {
        await using var db = CreateDbContext();
        AddTicket(db, TicketStatus.New, TicketPriority.Medium);
        AddTicket(db, TicketStatus.InProgress, TicketPriority.Critical);
        AddTicket(db, TicketStatus.Resolved, TicketPriority.Critical);
        await db.SaveChangesAsync();
        var service = new TicketService(db);

        var result = await service.GetStatisticsAsync(CancellationToken.None);

        Assert.Equal(3, result.Total);
        Assert.Equal(1, result.New);
        Assert.Equal(1, result.InProgress);
        Assert.Equal(1, result.Resolved);
        Assert.Equal(2, result.Critical);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static Ticket AddTicket(
        AppDbContext db,
        TicketStatus status,
        TicketPriority priority = TicketPriority.Medium,
        string title = "Sample ticket")
    {
        var ticket = new Ticket
        {
            Title = title,
            Description = "A sample ticket used by the unit test suite.",
            Priority = priority,
            Status = status,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5),
            UpdatedAtUtc = DateTime.UtcNow.AddMinutes(-5)
        };

        db.Tickets.Add(ticket);
        return ticket;
    }
}
