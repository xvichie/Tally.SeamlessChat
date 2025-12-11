using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SeamlessChat.Core.Dtos;
using SeamlessChat.Core.Interfaces;

namespace SeamlessChat.Realtime.Hubs;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly IChatService _chatService;
    private readonly IConversationService _conversationService;

    // connectionId -> userId
    private static readonly Dictionary<string, Guid> _connections = new();

    public ChatHub(
        IChatService chatService,
        IConversationService conversationService)
    {
        _chatService = chatService;
        _conversationService = conversationService;
    }

    private Guid? GetCurrentUserId()
    {
        var user = Context.User;
        if (user == null) return null;

        // Try common claim types
        var idClaim =
            user.FindFirst(ClaimTypes.NameIdentifier) ??
            user.FindFirst("sub") ??
            user.FindFirst("userId");

        if (idClaim == null) return null;

        return Guid.TryParse(idClaim.Value, out var id) ? id : null;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            _connections[Context.ConnectionId] = userId.Value;

            // Group per user for multi-device support
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.Value.ToString());
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryGetValue(Context.ConnectionId, out var userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
            _connections.Remove(Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // -------------------------------------------------------------
    // SEND MESSAGE
    // -------------------------------------------------------------
    public async Task SendMessage(SendMessageDto dto)
    {
        var currentUserId = GetCurrentUserId();

        // Optional: enforce sender identity from token instead of trusting dto.SenderId
        if (!currentUserId.HasValue || currentUserId.Value != dto.SenderId)
            throw new HubException("Unauthorized sender.");

        var messageDto = await _chatService.SendMessageAsync(dto);

        // Notify receiver
        await Clients.Group(dto.ReceiverId.ToString())
            .ReceiveMessage(messageDto);

        // Echo to sender as well
        await Clients.Group(dto.SenderId.ToString())
            .ReceiveMessage(messageDto);
    }

    // -------------------------------------------------------------
    // TYPING
    // -------------------------------------------------------------
    public async Task Typing(Guid receiverId)
    {
        var senderId = GetCurrentUserId();
        if (!senderId.HasValue) return;

        var conversationId =
            _conversationService.GetConversationId(senderId.Value, receiverId);

        await Clients.Group(receiverId.ToString())
            .TypingStarted(senderId.Value, conversationId);
    }

    // -------------------------------------------------------------
    // DELIVERED
    // -------------------------------------------------------------
    public async Task MarkDelivered(string conversationId, Guid messageId, Guid otherParticipantId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return;

        await _chatService.MarkDeliveredAsync(conversationId, messageId);

        await Clients.Group(otherParticipantId.ToString())
            .MessageDelivered(messageId, conversationId);
    }

    // -------------------------------------------------------------
    // SEEN
    // -------------------------------------------------------------
    public async Task MarkSeen(string conversationId, Guid messageId, Guid otherParticipantId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return;

        await _chatService.MarkSeenAsync(conversationId, messageId, userId.Value);

        await Clients.Group(otherParticipantId.ToString())
            .MessageSeen(messageId, conversationId);
    }
}
