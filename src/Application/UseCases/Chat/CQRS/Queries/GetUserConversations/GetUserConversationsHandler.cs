using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.UseCases.Chat.DTOs;
using Domain.Entities.Chat;

namespace Application.UseCases.Chat.CQRS.Queries.GetUserConversations;

public class GetUserConversationsHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<GetUserConversationsQuery, OperationResult<List<ConversationListItemDTO>>>
{
    public async Task<OperationResult<List<ConversationListItemDTO>>> Handle(
        GetUserConversationsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validate user ID
        if (request.UserId == Guid.Empty)
            return Result.Error(ErrorResult.BadRequest, ChatMessages.Validation.InvalidUserId);

        // 2. Verify user exists
        var user = await unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result.Error(ErrorResult.NotFound, ChatMessages.Validation.UserNotFound);

        // 3. Get all conversations for the user
        var conversations = await unitOfWork.Conversations.GetUserConversationsAsync(
            request.UserId, cancellationToken);

        // 4. Map to DTOs
        var conversationDtos = new List<ConversationListItemDTO>();

        foreach (var conversation in conversations)
        {
            // Get the other participant's ID
            var otherUserId = conversation.GetOtherParticipant(request.UserId);

            // Get the other user's info
            var otherUser = await unitOfWork.Users.GetByIdAsync(otherUserId, cancellationToken);
            if (otherUser == null)
                continue; // Skip if user not found (shouldn't happen)

            // Get last message preview
            var lastMessage = conversation.MessagesReadOnly
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefault();

            // Get unread count
            var unreadCount = await unitOfWork.Conversations.GetUnreadCountAsync(
                conversation.Id, request.UserId, cancellationToken);

            var dto = new ConversationListItemDTO(
                Id: conversation.Id,
                OtherUserId: otherUserId,
                OtherUserName: otherUser.Name,
                LastMessage: lastMessage?.Message,
                LastMessageAt: conversation.LastMessageAt,
                UnreadCount: unreadCount
            );

            conversationDtos.Add(dto);
        }

        // 5. Order by last message time (newest first)
        conversationDtos = conversationDtos
            .OrderByDescending(c => c.LastMessageAt)
            .ToList();

        return Result.Success(conversationDtos);
    }
}
