using Domain.Entities.Chat;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Conversation entity.
/// </summary>
public class ConversationRepository : RepositoryBase<Conversation>, IConversationRepository
{
    public ConversationRepository(SuperPOSDbContext context) : base(context)
    {
    }

    public async Task<Conversation?> GetConversationByUsersAsync(
        Guid user1Id,
        Guid user2Id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Conversation>()
            .FirstOrDefaultAsync(c =>
                (c.User1Id == user1Id && c.User2Id == user2Id) ||
                (c.User1Id == user2Id && c.User2Id == user1Id),
                cancellationToken);
    }

    public async Task<List<Conversation>> GetUserConversationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Conversation>()
            .Where(c => c.User1Id == userId || c.User2Id == userId)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Conversation?> GetConversationWithMessagesAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Conversation>()
            .Include(c => c.Messages.OrderByDescending(m => m.SentAt))
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<ChatMessage>()
            .Where(m => m.ConversationId == conversationId &&
                       m.SenderId != userId &&
                       !m.IsRead)
            .CountAsync(cancellationToken);
    }
}
