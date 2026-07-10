using Microsoft.AspNetCore.Mvc;
using SupportTicketManagementSystem.Application.Common.Models;
using SupportTicketManagementSystem.Application.DTOs.Tickets;
using SupportTicketManagementSystem.Application.Interfaces.Services;

namespace SupportTicketManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TicketDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResult<TicketDto>>>> Search(
        [FromQuery] TicketQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _ticketService.SearchAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<TicketDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TicketDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<TicketDto>.Ok(ticket));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Models.ErrorResponse), StatusCodes.Status400BadRequest)]
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

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TicketDto>>> Update(
        int id,
        [FromBody] UpdateTicketDto dto,
        CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<TicketDto>.Ok(ticket, "Ticket updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _ticketService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new object(), "Ticket deleted successfully."));
    }
}
