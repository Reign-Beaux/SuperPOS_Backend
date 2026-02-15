using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Chat.DTOs;

namespace Application.UseCases.Chat.CQRS.Commands.SendMessage;

/// <summary>
/// Command to send a chat message.
/// Creates conversation if it doesn't exist.
/// </summary>
/// <param name="SenderId">User sending the message</param>
/// <param name="RecipientId">User receiving the message</param>
/// <param name="Message">Message content</param>
public record SendMessageCommand(
    Guid SenderId,
    Guid RecipientId,
    string Message
) : IRequest<OperationResult<ChatMessageDTO>>;
