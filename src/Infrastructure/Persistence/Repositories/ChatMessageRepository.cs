using Domain.Entities.Chat;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for ChatMessage entity.
/// </summary>
public class ChatMessageRepository : RepositoryBase<ChatMessage>, IChatMessageRepository
{
    public ChatMessageRepository(SuperPOSDbContext context) : base(context)
    {
    }

    public async Task<List<ChatMessage>> GetConversationMessagesAsync(
        Guid conversationId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var skip = (pageIndex - 1) * pageSize;

        return await _context.Set<ChatMessage>()
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.SentAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetConversationMessageCountAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<ChatMessage>()
            .Where(m => m.ConversationId == conversationId)
            .CountAsync(cancellationToken);
    }
}
