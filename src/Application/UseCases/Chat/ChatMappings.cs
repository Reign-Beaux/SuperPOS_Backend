using Application.UseCases.Chat.DTOs;
using Domain.Entities.Chat;

namespace Application.UseCases.Chat;

/// <summary>
/// Mapster configuration for Chat entities.
/// </summary>
public class ChatMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ChatMessage -> ChatMessageDTO
        config.NewConfig<ChatMessage, ChatMessageDTO>()
            .Map(dest => dest.SenderName, src => "");  // Will be populated from User entity in handler

        // Conversation -> ConversationDTO
        config.NewConfig<Conversation, ConversationDTO>()
            .Map(dest => dest.User1Name, src => "")  // Will be populated from User entity in handler
            .Map(dest => dest.User2Name, src => "")  // Will be populated from User entity in handler
            .Map(dest => dest.UnreadCount, src => 0)  // Will be calculated in handler
            .Map(dest => dest.Messages, src => new List<ChatMessageDTO>());  // Will be populated in handler

        // Conversation -> ConversationListItemDTO
        config.NewConfig<Conversation, ConversationListItemDTO>()
            .Map(dest => dest.OtherUserId, src => Guid.Empty)  // Will be determined in handler
            .Map(dest => dest.OtherUserName, src => "")  // Will be populated from User entity in handler
            .Map(dest => dest.LastMessage, src => "")  // Will be extracted from last message in handler
            .Map(dest => dest.UnreadCount, src => 0);  // Will be calculated in handler
    }
}
