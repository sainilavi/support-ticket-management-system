using Microsoft.AspNetCore.Mvc;
using SupportTicketManagementSystem.Application.Common.Models;
using SupportTicketManagementSystem.Application.DTOs.Comments;
using SupportTicketManagementSystem.Application.Interfaces.Services;

namespace SupportTicketManagementSystem.API.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:int}/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CommentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CommentDto>>>> GetByTicketId(
        int ticketId,
        CancellationToken cancellationToken)
    {
        var comments = await _commentService.GetByTicketIdAsync(ticketId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CommentDto>>.Ok(comments));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Models.ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Models.ApiErrorResponse), StatusCodes.Status404NotFound)]
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
