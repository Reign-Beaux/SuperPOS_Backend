using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Chat.DTOs;

namespace Application.UseCases.Chat.CQRS.Queries.GetConversation;

/// <summary>
/// Query to get a conversation with all messages.
/// </summary>
/// <param name="ConversationId">Conversation ID</param>
/// <param name="UserId">User ID requesting the conversation (for authorization)</param>
public record GetConversationQuery(Guid ConversationId, Guid UserId) : IRequest<OperationResult<ConversationDTO>>;
