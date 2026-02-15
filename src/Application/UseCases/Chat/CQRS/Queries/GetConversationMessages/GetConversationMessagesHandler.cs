using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.UseCases.Chat.DTOs;
using Domain.Entities.Chat;

namespace Application.UseCases.Chat.CQRS.Queries.GetConversationMessages;

public class GetConversationMessagesHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<GetConversationMessagesQuery, OperationResult<List<ChatMessageDTO>>>
{
    public async Task<OperationResult<List<ChatMessageDTO>>> Handle(
        GetConversationMessagesQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validate IDs
        if (request.ConversationId == Guid.Empty || request.UserId == Guid.Empty)
            return Result.Error(ErrorResult.BadRequest, ChatMessages.Validation.InvalidConversationId);

        // 2. Validate pagination parameters
        if (request.PageIndex < 1 || request.PageSize < 1)
            return Result.Error(ErrorResult.BadRequest, "Parámetros de paginación inválidos");

        // 3. Get conversation to verify it exists and user is participant
        var conversation = await unitOfWork.Conversations.GetByIdAsync(
            request.ConversationId, cancellationToken);

        if (conversation == null)
            return Result.Error(ErrorResult.NotFound, ChatMessages.Conversation.WithId(request.ConversationId));

        // 4. Verify user is a participant
        if (!conversation.IsParticipant(request.UserId))
            return Result.Error(ErrorResult.Forbidden, ChatMessages.Conversation.UserNotParticipant);

        // 5. Get paginated messages
        var messages = await unitOfWork.ChatMessages.GetConversationMessagesAsync(
            request.ConversationId,
            request.PageIndex,
            request.PageSize,
            cancellationToken);

        // 6. Get users info for sender names
        var user1 = await unitOfWork.Users.GetByIdAsync(conversation.User1Id, cancellationToken);
        var user2 = await unitOfWork.Users.GetByIdAsync(conversation.User2Id, cancellationToken);

        if (user1 == null || user2 == null)
            return Result.Error(ErrorResult.NotFound, ChatMessages.Validation.UserNotFound);

        // 7. Map to DTOs
        var messageDtos = new List<ChatMessageDTO>();
        foreach (var message in messages)
        {
            var senderName = message.SenderId == user1.Id ? user1.Name : user2.Name;

            var messageDto = new ChatMessageDTO(
                Id: message.Id,
                ConversationId: message.ConversationId,
                SenderId: message.SenderId,
                SenderName: senderName,
                Message: message.Message,
                SentAt: message.SentAt,
                IsRead: message.IsRead,
                ReadAt: message.ReadAt
            );

            messageDtos.Add(messageDto);
        }

        return Result.Success(messageDtos);
    }
}
