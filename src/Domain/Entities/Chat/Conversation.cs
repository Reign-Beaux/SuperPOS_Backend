namespace Domain.Entities.Chat;

/// <summary>
/// Conversation between two users.
/// Aggregates all messages between a Manager/Admin and a Seller.
/// </summary>
public class Conversation : BaseEntity
{
    private readonly List<ChatMessage> _messages = [];

    // Parameterless constructor for EF Core
    public Conversation() { }

    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }
    public DateTime LastMessageAt { get; set; }

    // Navigation property
    public ICollection<ChatMessage> Messages
    {
        get => _messages;
        set
        {
            _messages.Clear();
            if (value != null)
            {
                foreach (var item in value)
                {
                    _messages.Add(item);
                }
            }
        }
    }

    public IReadOnlyCollection<ChatMessage> MessagesReadOnly => _messages.AsReadOnly();

    /// <summary>
    /// Factory method to create a new conversation.
    /// </summary>
    public static Conversation Create(Guid user1Id, Guid user2Id)
    {
        return new Conversation
        {
            User1Id = user1Id,
            User2Id = user2Id,
            LastMessageAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Adds a message to the conversation.
    /// </summary>
    public void AddMessage(Guid senderId, string message)
    {
        var chatMessage = ChatMessage.Create(Id, senderId, message);
        _messages.Add(chatMessage);
        LastMessageAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a user is participant of this conversation.
    /// </summary>
    public bool IsParticipant(Guid userId)
    {
        return User1Id == userId || User2Id == userId;
    }

    /// <summary>
    /// Gets the other participant's ID.
    /// </summary>
    public Guid GetOtherParticipant(Guid userId)
    {
        return userId == User1Id ? User2Id : User1Id;
    }
}
