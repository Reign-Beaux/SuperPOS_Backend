namespace Application.UseCases.Chat.DTOs;

/// <summary>
/// DTO for chat message.
/// </summary>
/// <param name="Id">Message ID</param>
/// <param name="ConversationId">Conversation ID</param>
/// <param name="SenderId">Sender user ID</param>
/// <param name="SenderName">Sender user name</param>
/// <param name="Message">Message content</param>
/// <param name="SentAt">Timestamp when message was sent</param>
/// <param name="IsRead">Whether the message has been read</param>
/// <param name="ReadAt">Timestamp when message was read (if applicable)</param>
public record ChatMessageDTO(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string SenderName,
    string Message,
    DateTime SentAt,
    bool IsRead,
    DateTime? ReadAt
);
