using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Chat.CQRS.Commands.MarkMessageAsRead;

/// <summary>
/// Command to mark a chat message as read.
/// </summary>
/// <param name="MessageId">Message ID to mark as read</param>
/// <param name="UserId">User marking the message as read (for authorization)</param>
public record MarkMessageAsReadCommand(
    Guid MessageId,
    Guid UserId
) : IRequest<OperationResult<bool>>;
