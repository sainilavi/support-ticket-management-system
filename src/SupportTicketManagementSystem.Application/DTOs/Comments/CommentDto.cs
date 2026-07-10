namespace SupportTicketManagementSystem.Application.DTOs.Comments;

/// <summary>
/// Comment details returned by the API.
/// </summary>
public class CommentDto
{
    /// <summary>Comment identifier.</summary>
    public int Id { get; set; }

    /// <summary>Comment text.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Associated ticket identifier.</summary>
    public int TicketId { get; set; }

    /// <summary>Identifier of the user who posted the comment.</summary>
    public int UserId { get; set; }

    /// <summary>Display name of the user who posted the comment.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the comment was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp when the comment was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }
}
