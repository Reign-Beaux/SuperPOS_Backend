namespace Application.UseCases.Chat.DTOs;

/// <summary>
/// DTO for conversation with messages.
/// </summary>
/// <param name="Id">Conversation ID</param>
/// <param name="User1Id">First participant ID</param>
/// <param name="User1Name">First participant name</param>
/// <param name="User2Id">Second participant ID</param>
/// <param name="User2Name">Second participant name</param>
/// <param name="LastMessageAt">Timestamp of last message</param>
/// <param name="UnreadCount">Count of unread messages for current user</param>
/// <param name="Messages">List of messages in conversation</param>
public record ConversationDTO(
    Guid Id,
    Guid User1Id,
    string User1Name,
    Guid User2Id,
    string User2Name,
    DateTime LastMessageAt,
    int UnreadCount,
    List<ChatMessageDTO> Messages
);
