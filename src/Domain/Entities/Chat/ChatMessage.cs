namespace Domain.Entities.Chat;

/// <summary>
/// Individual chat message between users.
/// Part of real-time messaging system using WebSockets.
/// </summary>
public class ChatMessage : BaseEntity
{
    // Parameterless constructor for EF Core
    public ChatMessage() { }

    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    // Navigation property
    public Conversation Conversation { get; set; } = null!;

    /// <summary>
    /// Factory method to create a new chat message.
    /// </summary>
    public static ChatMessage Create(
        Guid conversationId,
        Guid senderId,
        string message)
    {
        return new ChatMessage
        {
            ConversationId = conversationId,
            SenderId = senderId,
            Message = message,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };
    }

    /// <summary>
    /// Marks the message as read.
    /// </summary>
    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
