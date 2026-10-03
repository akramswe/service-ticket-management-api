using ServiceTicketManagement.Api.Domain;

namespace ServiceTicketManagement.Api.DTOs;

public sealed record TicketResponse(
    Guid Id,
    string Title,
    string Description,
    TicketPriority Priority,
    TicketStatus Status,
    string? Assignee,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<TicketCommentResponse> Comments);
