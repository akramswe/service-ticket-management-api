using System.ComponentModel.DataAnnotations;
using ServiceTicketManagement.Api.Domain;

namespace ServiceTicketManagement.Api.DTOs;

public sealed class UpdateTicketRequest
{
    [Required, StringLength(120, MinimumLength = 3)]
    public string Title { get; init; } = string.Empty;

    [Required, StringLength(4000, MinimumLength = 5)]
    public string Description { get; init; } = string.Empty;

    public TicketPriority Priority { get; init; }

    [StringLength(100)]
    public string? Assignee { get; init; }
}
