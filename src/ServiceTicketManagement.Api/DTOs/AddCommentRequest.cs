using System.ComponentModel.DataAnnotations;

namespace ServiceTicketManagement.Api.DTOs;

public sealed class AddCommentRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Author { get; init; } = string.Empty;

    [Required, StringLength(1000, MinimumLength = 2)]
    public string Message { get; init; } = string.Empty;
}
