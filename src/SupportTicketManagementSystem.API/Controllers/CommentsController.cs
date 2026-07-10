using Microsoft.AspNetCore.Mvc;
using SupportTicketManagementSystem.Application.Common.Models;
using SupportTicketManagementSystem.Application.DTOs.Comments;
using SupportTicketManagementSystem.Application.Interfaces.Services;

namespace SupportTicketManagementSystem.API.Controllers;

/// <summary>
/// Provides endpoints for managing ticket comments.
/// </summary>
[ApiController]
[Route("api/tickets/{ticketId:int}/comments")]
[Tags("Comments")]
[ApiExplorerSettings(GroupName = "comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// Gets all comments for a ticket ordered by creation date.
    /// </summary>
    /// <param name="ticketId">Ticket identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Comments for the specified ticket.</returns>
    /// <response code="200">Returns the ticket comments.</response>
    /// <response code="400">Ticket validation failed.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CommentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CommentDto>>>> GetByTicketId(
        int ticketId,
        CancellationToken cancellationToken)
    {
        var comments = await _commentService.GetByTicketIdAsync(ticketId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CommentDto>>.Ok(comments));
    }

    /// <summary>
    /// Adds a comment to a ticket.
    /// </summary>
    /// <param name="ticketId">Ticket identifier.</param>
    /// <param name="dto">Comment creation payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created comment.</returns>
    /// <response code="201">Comment added successfully.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Models.ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CommentDto>>> Create(
        int ticketId,
        [FromBody] CreateCommentDto dto,
        CancellationToken cancellationToken)
    {
        var comment = await _commentService.CreateAsync(ticketId, dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetByTicketId),
            new { ticketId },
            ApiResponse<CommentDto>.Ok(comment, "Comment added successfully."));
    }
}
