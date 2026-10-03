namespace ServiceTicketManagement.Api.DTOs;

public sealed record TicketStatisticsResponse(
    int Total,
    int New,
    int InProgress,
    int Resolved,
    int Closed,
    int Unassigned,
    int Critical);
