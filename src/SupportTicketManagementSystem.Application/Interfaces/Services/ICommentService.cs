using SupportTicketManagementSystem.Application.DTOs.Comments;

namespace SupportTicketManagementSystem.Application.Interfaces.Services;

public interface ICommentService
{
    Task<IReadOnlyList<CommentDto>> GetByTicketIdAsync(int ticketId, CancellationToken cancellationToken = default);
    Task<CommentDto> CreateAsync(int ticketId, CreateCommentDto dto, CancellationToken cancellationToken = default);
}
