using Microsoft.AspNetCore.Mvc;
using ServiceTicketManagement.Api.Domain;
using ServiceTicketManagement.Api.DTOs;
using ServiceTicketManagement.Api.Services;

namespace ServiceTicketManagement.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController(ITicketService ticketService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TicketResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TicketResponse>>> GetAll(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] string? assignee,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var tickets = await ticketService.GetAllAsync(status, priority, assignee, search, cancellationToken);
        return Ok(tickets);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetByIdAsync(id, cancellationToken);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpGet("statistics")]
    [ProducesResponseType<TicketStatisticsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketStatisticsResponse>> GetStatistics(CancellationToken cancellationToken)
    {
        return Ok(await ticketService.GetStatisticsAsync(cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketResponse>> Create(
        CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var created = await ticketService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> Update(
        Guid id,
        UpdateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await ticketService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketResponse>> ChangeStatus(
        Guid id,
        UpdateTicketStatusRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await ticketService.ChangeStatusAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType<TicketCommentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketCommentResponse>> AddComment(
        Guid id,
        AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        var comment = await ticketService.AddCommentAsync(id, request, cancellationToken);
        return comment is null ? NotFound() : Ok(comment);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await ticketService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
