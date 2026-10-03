using System.ComponentModel.DataAnnotations;
using ServiceTicketManagement.Api.Domain;

namespace ServiceTicketManagement.Api.DTOs;

public sealed class UpdateTicketStatusRequest
{
    [Required]
    public TicketStatus? Status { get; init; }
}
