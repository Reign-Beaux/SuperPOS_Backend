using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Chat.CQRS.Commands.MarkMessageAsRead;
using Application.UseCases.Chat.CQRS.Commands.SendMessage;
using Application.UseCases.Chat.CQRS.Queries.GetConversation;
using Application.UseCases.Chat.CQRS.Queries.GetConversationMessages;
using Application.UseCases.Chat.CQRS.Queries.GetUserConversations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Web.API.Controllers;

/// <summary>
/// Chat controller for real-time messaging between users.
/// Provides REST endpoints for chat operations.
/// </summary>
[Route("api/[controller]")]
[Authorize(Policy = "SellerOrAbove")]
public class ChatController(IMediator mediator) : BaseController
{
    /// <summary>
    /// Gets all conversations for the current user.
    /// </summary>
    /// <returns>List of conversation summaries</returns>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = GetCurrentUserId();
        var query = new GetUserConversationsQuery(userId);
        var result = await mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Gets a specific conversation with all messages.
    /// </summary>
    /// <param name="id">Conversation ID</param>
    /// <returns>Conversation with messages</returns>
    [HttpGet("conversations/{id}")]
    public async Task<IActionResult> GetConversation(Guid id)
    {
        var userId = GetCurrentUserId();
        var query = new GetConversationQuery(id, userId);
        var result = await mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Gets paginated messages for a conversation.
    /// </summary>
    /// <param name="id">Conversation ID</param>
    /// <param name="pageIndex">Page index (1-based, default: 1)</param>
    /// <param name="pageSize">Page size (default: 50)</param>
    /// <returns>Paginated list of messages</returns>
    [HttpGet("conversations/{id}/messages")]
    public async Task<IActionResult> GetConversationMessages(
        Guid id,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 50)
    {
        var userId = GetCurrentUserId();
        var query = new GetConversationMessagesQuery(id, userId, pageIndex, pageSize);
        var result = await mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Sends a message to another user.
    /// Creates conversation if it doesn't exist.
    /// </summary>
    /// <param name="command">Send message command</param>
    /// <returns>Created message</returns>
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageCommand command)
    {
        var userId = GetCurrentUserId();

        // Validate that the sender ID matches the current user
        if (command.SenderId != userId)
            return BadRequest("No puedes enviar mensajes en nombre de otro usuario");

        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Marks a message as read.
    /// </summary>
    /// <param name="id">Message ID</param>
    /// <returns>Updated message</returns>
    [HttpPut("messages/{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetCurrentUserId();
        var command = new MarkMessageAsReadCommand(id, userId);
        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Gets the current user ID from JWT claims.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario");

        return userId;
    }
}
