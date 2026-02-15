using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Chat.DTOs;

namespace Application.UseCases.Chat.CQRS.Queries.GetUserConversations;

/// <summary>
/// Query to get all conversations for a specific user.
/// </summary>
/// <param name="UserId">User ID to get conversations for</param>
public record GetUserConversationsQuery(Guid UserId) : IRequest<OperationResult<List<ConversationListItemDTO>>>;
