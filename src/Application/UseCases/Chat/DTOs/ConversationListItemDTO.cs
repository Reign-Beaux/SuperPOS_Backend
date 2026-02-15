namespace Application.UseCases.Chat.DTOs;

/// <summary>
/// DTO for conversation list item (summary view without messages).
/// </summary>
/// <param name="Id">Conversation ID</param>
/// <param name="OtherUserId">The other participant's ID</param>
/// <param name="OtherUserName">The other participant's name</param>
/// <param name="LastMessage">Preview of last message</param>
/// <param name="LastMessageAt">Timestamp of last message</param>
/// <param name="UnreadCount">Count of unread messages</param>
public record ConversationListItemDTO(
    Guid Id,
    Guid OtherUserId,
    string OtherUserName,
    string? LastMessage,
    DateTime LastMessageAt,
    int UnreadCount
);
