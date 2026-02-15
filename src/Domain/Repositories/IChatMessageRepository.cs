using Domain.Entities.Chat;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for ChatMessage entity with specialized query methods.
/// </summary>
public interface IChatMessageRepository : IRepositoryBase<ChatMessage>
{
    /// <summary>
    /// Gets paginated messages for a conversation, ordered by sent time (newest first).
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="pageIndex">Page index (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of messages</returns>
    Task<List<ChatMessage>> GetConversationMessagesAsync(
        Guid conversationId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of messages in a conversation.
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total message count</returns>
    Task<int> GetConversationMessageCountAsync(Guid conversationId, CancellationToken cancellationToken = default);
}
