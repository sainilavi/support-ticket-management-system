using Microsoft.AspNetCore.Mvc;
using SupportTicketManagementSystem.Application.Common.Models;
using SupportTicketManagementSystem.Application.DTOs.Tickets;
using SupportTicketManagementSystem.Application.Interfaces.Services;

namespace SupportTicketManagementSystem.API.Controllers;

/// <summary>
/// Provides endpoints for managing support tickets.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Tickets")]
[ApiExplorerSettings(GroupName = "tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>
    /// Searches tickets by keyword and status with pagination.
    /// </summary>
    /// <param name="query">Search filters and pagination options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of tickets.</returns>
    /// <response code="200">Returns the matching tickets.</response>
    /// <response code="400">Validation failed.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TicketDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResult<TicketDto>>>> Search(
        [FromQuery] TicketQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _ticketService.SearchAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<TicketDto>>.Ok(result));
    }

    /// <summary>
    /// Gets a ticket by identifier.
    /// </summary>
    /// <param name="id">Ticket identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested ticket.</returns>
    /// <response code="200">Returns the ticket.</response>
    /// <response code="404">Ticket was not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TicketDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<TicketDto>.Ok(ticket));
    }

    /// <summary>
    /// Creates a new support ticket.
    /// </summary>
    /// <param name="dto">Ticket creation payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created ticket.</returns>
    /// <response code="201">Ticket created successfully.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Models.ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TicketDto>>> Create(
        [FromBody] CreateTicketDto dto,
        CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = ticket.Id },
            ApiResponse<TicketDto>.Ok(ticket, "Ticket created successfully."));
    }

    /// <summary>
    /// Updates an existing ticket.
    /// </summary>
    /// <param name="id">Ticket identifier.</param>
    /// <param name="dto">Ticket update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated ticket.</returns>
    /// <response code="200">Ticket updated successfully.</response>
    /// <response code="400">Validation failed or status transition is invalid.</response>
    /// <response code="404">Ticket was not found.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Models.ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TicketDto>>> Update(
        int id,
        [FromBody] UpdateTicketDto dto,
        CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<TicketDto>.Ok(ticket, "Ticket updated successfully."));
    }

    /// <summary>
    /// Deletes a ticket by identifier.
    /// </summary>
    /// <param name="id">Ticket identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deletion confirmation.</returns>
    /// <response code="204">Ticket deleted successfully.</response>
    /// <response code="404">Ticket was not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Models.ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _ticketService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
