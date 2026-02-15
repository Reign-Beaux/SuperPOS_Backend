using Domain.Entities.Chat;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Conversation entity with specialized query methods.
/// </summary>
public interface IConversationRepository : IRepositoryBase<Conversation>
{
    /// <summary>
    /// Gets an existing conversation between two users.
    /// </summary>
    /// <param name="user1Id">First user ID</param>
    /// <param name="user2Id">Second user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Conversation if exists, null otherwise</returns>
    Task<Conversation?> GetConversationByUsersAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all conversations for a specific user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of conversations ordered by last message time</returns>
    Task<List<Conversation>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a conversation with all messages eagerly loaded.
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Conversation with messages, or null if not found</returns>
    Task<Conversation?> GetConversationWithMessagesAsync(Guid conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of unread messages in a conversation for a specific user.
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of unread messages</returns>
    Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
}
