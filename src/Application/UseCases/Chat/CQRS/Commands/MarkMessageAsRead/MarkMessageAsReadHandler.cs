using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Entities.Chat;

namespace Application.UseCases.Chat.CQRS.Commands.MarkMessageAsRead;

public class MarkMessageAsReadHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<MarkMessageAsReadCommand, OperationResult<bool>>
{
    public async Task<OperationResult<bool>> Handle(
        MarkMessageAsReadCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get message
        var message = await unitOfWork.ChatMessages.GetByIdAsync(request.MessageId, cancellationToken);

        if (message == null)
            return Result.Error(ErrorResult.NotFound, ChatMessages.Message.NotFound);

        // 2. Verify user is the recipient (not the sender)
        if (message.SenderId == request.UserId)
            return Result.Error(ErrorResult.BadRequest, "No puedes marcar como leído tu propio mensaje");

        // 3. Get conversation to verify user is a participant
        var conversation = await unitOfWork.Conversations.GetByIdAsync(message.ConversationId, cancellationToken);

        if (conversation == null)
            return Result.Error(ErrorResult.NotFound, ChatMessages.Conversation.NotFound);

        if (!conversation.IsParticipant(request.UserId))
            return Result.Error(ErrorResult.Forbidden, ChatMessages.Conversation.UserNotParticipant);

        // 4. Mark as read
        message.MarkAsRead();

        // 5. Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
