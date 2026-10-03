using ServiceTicketManagement.Api.Domain;

namespace ServiceTicketManagement.Api.Infrastructure;

public sealed class InvalidTicketStatusTransitionException(TicketStatus current, TicketStatus requested)
    : Exception($"Ticket status cannot change from '{current}' to '{requested}'.")
{
    public TicketStatus Current { get; } = current;
    public TicketStatus Requested { get; } = requested;
}
