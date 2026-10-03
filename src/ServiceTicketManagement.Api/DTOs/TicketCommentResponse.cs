namespace ServiceTicketManagement.Api.DTOs;

public sealed record TicketCommentResponse(
    Guid Id,
    string Author,
    string Message,
    DateTime CreatedAtUtc);
