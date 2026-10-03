using ServiceTicketManagement.Api.Domain;
using ServiceTicketManagement.Api.DTOs;

namespace ServiceTicketManagement.Api.Services;

public interface ITicketService
{
    Task<IReadOnlyList<TicketResponse>> GetAllAsync(
        TicketStatus? status,
        TicketPriority? priority,
        string? assignee,
        string? search,
        CancellationToken cancellationToken);

    Task<TicketResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TicketResponse> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken);
    Task<TicketResponse?> UpdateAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken);
    Task<TicketResponse?> ChangeStatusAsync(Guid id, UpdateTicketStatusRequest request, CancellationToken cancellationToken);
    Task<TicketCommentResponse?> AddCommentAsync(Guid id, AddCommentRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<TicketStatisticsResponse> GetStatisticsAsync(CancellationToken cancellationToken);
}
