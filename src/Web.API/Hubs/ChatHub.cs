using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Chat.CQRS.Commands.MarkMessageAsRead;
using Application.UseCases.Chat.CQRS.Commands.SendMessage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Web.API.Hubs;

/// <summary>
/// SignalR Hub for real-time chat functionality.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IMediator mediator, ILogger<ChatHub> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Joins a conversation group to receive real-time messages.
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        if (Guid.TryParse(conversationId, out var conversationGuid))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
            _logger.LogInformation(
                "User {UserId} joined conversation {ConversationId}",
                GetCurrentUserId(),
                conversationId);
        }
    }

    /// <summary>
    /// Leaves a conversation group.
    /// </summary>
    public async Task LeaveConversation(string conversationId)
    {
        if (Guid.TryParse(conversationId, out var conversationGuid))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
            _logger.LogInformation(
                "User {UserId} left conversation {ConversationId}",
                GetCurrentUserId(),
                conversationId);
        }
    }

    /// <summary>
    /// Sends a message to another user.
    /// Creates conversation if it doesn't exist.
    /// </summary>
    public async Task SendMessage(string recipientId, string message)
    {
        var senderId = GetCurrentUserId();

        if (!Guid.TryParse(recipientId, out var recipientGuid))
        {
            await Clients.Caller.SendAsync("Error", "ID de destinatario inválido");
            return;
        }

        var command = new SendMessageCommand(senderId, recipientGuid, message);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync("Error", result.Error?.Detail ?? "Error al enviar mensaje");
            return;
        }

        // Notify the conversation group
        var conversationId = result.Value!.ConversationId.ToString();
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);

        await Clients.Group(conversationId).SendAsync("ReceiveMessage", result.Value);

        _logger.LogInformation(
            "Message sent from {SenderId} to {RecipientId} in conversation {ConversationId}",
            senderId,
            recipientGuid,
            conversationId);
    }

    /// <summary>
    /// Marks a message as read.
    /// </summary>
    public async Task MarkAsRead(string messageId)
    {
        var userId = GetCurrentUserId();

        if (!Guid.TryParse(messageId, out var messageGuid))
        {
            await Clients.Caller.SendAsync("Error", "ID de mensaje inválido");
            return;
        }

        var command = new MarkMessageAsReadCommand(messageGuid, userId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync("Error", result.Error?.Detail ?? "Error al marcar mensaje como leído");
            return;
        }

        // Notify that the message was read (just send the message ID and user ID)
        await Clients.Caller.SendAsync("MessageMarkedAsRead", new { MessageId = messageGuid, UserId = userId });

        _logger.LogInformation(
            "Message {MessageId} marked as read by user {UserId}",
            messageGuid,
            userId);
    }

    /// <summary>
    /// Handles user connection.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} connected to chat hub", userId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Handles user disconnection.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation(
            "User {UserId} disconnected from chat hub. Exception: {Exception}",
            userId,
            exception?.Message ?? "None");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Gets the current user's ID from claims.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Could not extract user ID from claims");
            return Guid.Empty;
        }

        return userId;
    }
}
