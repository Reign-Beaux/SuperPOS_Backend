using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Chat.DTOs;

namespace Application.UseCases.Chat.CQRS.Queries.GetConversationMessages;

/// <summary>
/// Query to get paginated messages for a conversation.
/// </summary>
/// <param name="ConversationId">Conversation ID</param>
/// <param name="UserId">User ID requesting the messages (for authorization)</param>
/// <param name="PageIndex">Page index (1-based)</param>
/// <param name="PageSize">Page size</param>
public record GetConversationMessagesQuery(
    Guid ConversationId,
    Guid UserId,
    int PageIndex = 1,
    int PageSize = 50
) : IRequest<OperationResult<List<ChatMessageDTO>>>;
