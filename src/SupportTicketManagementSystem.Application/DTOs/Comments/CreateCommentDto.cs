namespace SupportTicketManagementSystem.Application.DTOs.Comments;

/// <summary>
/// Request payload for creating a ticket comment.
/// </summary>
public class CreateCommentDto
{
    /// <summary>Comment text (max 2000 characters).</summary>
    /// <example>We are investigating this issue and will update you shortly.</example>
    public string Content { get; set; } = string.Empty;

    /// <summary>Identifier of the user posting the comment.</summary>
    /// <example>2</example>
    public int UserId { get; set; }
}
