using Microsoft.EntityFrameworkCore;
using ServiceTicketManagement.Api.Data;
using ServiceTicketManagement.Api.Domain;
using ServiceTicketManagement.Api.DTOs;
using ServiceTicketManagement.Api.Infrastructure;

namespace ServiceTicketManagement.Api.Services;

public sealed class TicketService(AppDbContext db) : ITicketService
{
    public async Task<IReadOnlyList<TicketResponse>> GetAllAsync(
        TicketStatus? status,
        TicketPriority? priority,
        string? assignee,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = db.Tickets
            .AsNoTracking()
            .Include(x => x.Comments)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(x => x.Priority == priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(assignee))
        {
            var normalizedAssignee = assignee.Trim().ToLower();
            query = query.Where(x => x.Assignee != null && x.Assignee.ToLower() == normalizedAssignee);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(normalizedSearch) ||
                x.Description.ToLower().Contains(normalizedSearch));
        }

        var tickets = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return tickets.Select(ToResponse).ToList();
    }

    public async Task<TicketResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await db.Tickets
            .AsNoTracking()
            .Include(x => x.Comments)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return ticket is null ? null : ToResponse(ticket);
    }

    public async Task<TicketResponse> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var ticket = new Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            Assignee = NormalizeOptional(request.Assignee),
            Status = TicketStatus.New,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(ticket);
    }

    public async Task<TicketResponse?> UpdateAsync(
        Guid id,
        UpdateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await db.Tickets
            .Include(x => x.Comments)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ticket is null)
        {
            return null;
        }

        ticket.Title = request.Title.Trim();
        ticket.Description = request.Description.Trim();
        ticket.Priority = request.Priority;
        ticket.Assignee = NormalizeOptional(request.Assignee);
        ticket.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(ticket);
    }

    public async Task<TicketResponse?> ChangeStatusAsync(
        Guid id,
        UpdateTicketStatusRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await db.Tickets
            .Include(x => x.Comments)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ticket is null)
        {
            return null;
        }

        var requestedStatus = request.Status
            ?? throw new ArgumentException("Status is required.");

        if (ticket.Status == requestedStatus)
        {
            return ToResponse(ticket);
        }

        if (!IsValidTransition(ticket.Status, requestedStatus))
        {
            throw new InvalidTicketStatusTransitionException(ticket.Status, requestedStatus);
        }

        ticket.Status = requestedStatus;
        ticket.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(ticket);
    }

    public async Task<TicketCommentResponse?> AddCommentAsync(
        Guid id,
        AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await db.Tickets.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ticket is null)
        {
            return null;
        }

        var comment = new TicketComment
        {
            TicketId = ticket.Id,
            Author = request.Author.Trim(),
            Message = request.Message.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        db.TicketComments.Add(comment);
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return ToCommentResponse(comment);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await db.Tickets.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ticket is null)
        {
            return false;
        }

        db.Tickets.Remove(ticket);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<TicketStatisticsResponse> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var tickets = await db.Tickets
            .AsNoTracking()
            .Select(x => new { x.Status, x.Priority, x.Assignee })
            .ToListAsync(cancellationToken);

        return new TicketStatisticsResponse(
            Total: tickets.Count,
            New: tickets.Count(x => x.Status == TicketStatus.New),
            InProgress: tickets.Count(x => x.Status == TicketStatus.InProgress),
            Resolved: tickets.Count(x => x.Status == TicketStatus.Resolved),
            Closed: tickets.Count(x => x.Status == TicketStatus.Closed),
            Unassigned: tickets.Count(x => string.IsNullOrWhiteSpace(x.Assignee)),
            Critical: tickets.Count(x => x.Priority == TicketPriority.Critical));
    }

    private static bool IsValidTransition(TicketStatus current, TicketStatus requested) => current switch
    {
        TicketStatus.New => requested is TicketStatus.InProgress or TicketStatus.Closed,
        TicketStatus.InProgress => requested is TicketStatus.Resolved or TicketStatus.Closed,
        TicketStatus.Resolved => requested is TicketStatus.InProgress or TicketStatus.Closed,
        TicketStatus.Closed => false,
        _ => false
    };

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static TicketResponse ToResponse(Ticket ticket) => new(
        ticket.Id,
        ticket.Title,
        ticket.Description,
        ticket.Priority,
        ticket.Status,
        ticket.Assignee,
        ticket.CreatedAtUtc,
        ticket.UpdatedAtUtc,
        ticket.Comments
            .OrderBy(x => x.CreatedAtUtc)
            .Select(ToCommentResponse)
            .ToList());

    private static TicketCommentResponse ToCommentResponse(TicketComment comment) => new(
        comment.Id,
        comment.Author,
        comment.Message,
        comment.CreatedAtUtc);
}
