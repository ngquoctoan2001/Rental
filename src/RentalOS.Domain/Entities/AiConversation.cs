namespace RentalOS.Domain.Entities;

/// <summary>A chat history between a user and the AI assistant — bảng 13.</summary>
public class AiConversation : BaseEntity
{
    /// <summary>FK to the user who started this conversation.</summary>
    public Guid UserId { get; set; }

    /// <summary>Auto-generated summary / title of the conversation. MỚI.</summary>
    public string? Title { get; set; }

    /// <summary>JSON array of message objects (role + content).</summary>
    public string Messages { get; set; } = "[]";

    /// <summary>Total number of messages exchanged.</summary>
    public int MessageCount { get; set; } = 0;

    /// <summary>Timestamp of the most recent message. MỚI.</summary>
    public DateTime? LastMessageAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
