using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Chat.DTOs;
using Domain.Entities.Chat;

namespace Application.UseCases.Chat.CQRS.Commands.SendMessage;

public class SendMessageHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<SendMessageCommand, OperationResult<ChatMessageDTO>>
{
    public async Task<OperationResult<ChatMessageDTO>> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate
        if (request.SenderId == Guid.Empty || request.RecipientId == Guid.Empty)
            return Result.Error(ErrorResult.BadRequest, ChatMessages.Validation.InvalidUserId);

        if (request.SenderId == request.RecipientId)
            return Result.Error(ErrorResult.BadRequest, ChatMessages.Validation.SameUser);

        if (string.IsNullOrWhiteSpace(request.Message))
            return Result.Error(ErrorResult.BadRequest, ChatMessages.Message.Empty);

        if (request.Message.Length > 2000)
            return Result.Error(ErrorResult.BadRequest, ChatMessages.Message.TooLong);

        // 2. Verify users exist
        var sender = await unitOfWork.Users.GetByIdAsync(request.SenderId, cancellationToken);
        if (sender == null)
            return Result.Error(ErrorResult.NotFound, ChatMessages.Validation.UserNotFound);

        var recipient = await unitOfWork.Users.GetByIdAsync(request.RecipientId, cancellationToken);
        if (recipient == null)
            return Result.Error(ErrorResult.NotFound, ChatMessages.Validation.UserNotFound);

        // 3. Get or create conversation
        var conversation = await unitOfWork.Conversations.GetConversationByUsersAsync(
            request.SenderId, request.RecipientId, cancellationToken);

        if (conversation == null)
        {
            conversation = Conversation.Create(request.SenderId, request.RecipientId);
            unitOfWork.Repository<Conversation>().Add(conversation);
        }

        // 4. Add message to conversation
        conversation.AddMessage(request.SenderId, request.Message);

        // 5. Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Get the last message (the one we just added)
        var lastMessage = conversation.MessagesReadOnly.OrderByDescending(m => m.SentAt).FirstOrDefault();

        if (lastMessage == null)
            return Result.Error(ErrorResult.InternalServerError, "Error al enviar el mensaje");

        // 7. Map to DTO
        var messageDto = new ChatMessageDTO(
            Id: lastMessage.Id,
            ConversationId: conversation.Id,
            SenderId: lastMessage.SenderId,
            SenderName: sender.Name,
            Message: lastMessage.Message,
            SentAt: lastMessage.SentAt,
            IsRead: lastMessage.IsRead,
            ReadAt: lastMessage.ReadAt
        );

        return Result.Success(messageDto);
    }
}
