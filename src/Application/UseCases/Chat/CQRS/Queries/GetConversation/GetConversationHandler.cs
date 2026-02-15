using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.UseCases.Chat.DTOs;
using Domain.Entities.Chat;

namespace Application.UseCases.Chat.CQRS.Queries.GetConversation;

public class GetConversationHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<GetConversationQuery, OperationResult<ConversationDTO>>
{
    public async Task<OperationResult<ConversationDTO>> Handle(
        GetConversationQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validate IDs
        if (request.ConversationId == Guid.Empty || request.UserId == Guid.Empty)
            return Result.Error(ErrorResult.BadRequest, ChatMessages.Validation.InvalidConversationId);

        // 2. Get conversation with messages
        var conversation = await unitOfWork.Conversations.GetConversationWithMessagesAsync(
            request.ConversationId, cancellationToken);

        if (conversation == null)
            return Result.Error(ErrorResult.NotFound, ChatMessages.Conversation.WithId(request.ConversationId));

        // 3. Verify user is a participant
        if (!conversation.IsParticipant(request.UserId))
            return Result.Error(ErrorResult.Forbidden, ChatMessages.Conversation.UserNotParticipant);

        // 4. Get both users' info
        var user1 = await unitOfWork.Users.GetByIdAsync(conversation.User1Id, cancellationToken);
        var user2 = await unitOfWork.Users.GetByIdAsync(conversation.User2Id, cancellationToken);

        if (user1 == null || user2 == null)
            return Result.Error(ErrorResult.NotFound, ChatMessages.Validation.UserNotFound);

        // 5. Get unread count for current user
        var unreadCount = await unitOfWork.Conversations.GetUnreadCountAsync(
            conversation.Id, request.UserId, cancellationToken);

        // 6. Map messages to DTOs
        var messageDtos = new List<ChatMessageDTO>();
        foreach (var message in conversation.MessagesReadOnly.OrderBy(m => m.SentAt))
        {
            // Get sender name
            var senderName = message.SenderId == user1.Id ? user1.Name : user2.Name;

            var messageDto = new ChatMessageDTO(
                Id: message.Id,
                ConversationId: conversation.Id,
                SenderId: message.SenderId,
                SenderName: senderName,
                Message: message.Message,
                SentAt: message.SentAt,
                IsRead: message.IsRead,
                ReadAt: message.ReadAt
            );

            messageDtos.Add(messageDto);
        }

        // 7. Create conversation DTO
        var conversationDto = new ConversationDTO(
            Id: conversation.Id,
            User1Id: conversation.User1Id,
            User1Name: user1.Name,
            User2Id: conversation.User2Id,
            User2Name: user2.Name,
            LastMessageAt: conversation.LastMessageAt,
            UnreadCount: unreadCount,
            Messages: messageDtos
        );

        return Result.Success(conversationDto);
    }
}
