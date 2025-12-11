namespace SeamlessChat.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeamlessChat.Api.Extensions;
using SeamlessChat.Core.Dtos;
using SeamlessChat.Core.Interfaces;

[ApiController]
[Route("chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IConversationService _conversationService;

    public ChatController(
        IChatService chatService,
        IConversationService conversationService)
    {
        _chatService = chatService;
        _conversationService = conversationService;
    }

    // ---------------------------------------------------------
    // SEND MESSAGE
    // ---------------------------------------------------------
    [HttpPost("send")]
    public async Task<ActionResult<MessageDto>> Send([FromBody] SendMessageDto dto)
    {
        var senderId = User.GetUserId();

        if (dto.SenderId != senderId)
            return Unauthorized();

        var message = await _chatService.SendMessageAsync(dto);
        return Ok(message);
    }

    // ---------------------------------------------------------
    // LOAD MESSAGES (cursor pagination)
    // ---------------------------------------------------------
    [HttpGet("messages")]
    public async Task<ActionResult<LoadMessagesResponseDto>> LoadMessages(
        [FromQuery] Guid user1Id,
        [FromQuery] Guid user2Id,
        [FromQuery] int limit = 40,
        [FromQuery] long? before = null)
    {
        var currentUser = User.GetUserId();

        // Validate user is allowed
        if (currentUser != user1Id && currentUser != user2Id)
            return Unauthorized();

        var dto = new LoadMessagesRequestDto
        {
            User1Id = user1Id,
            User2Id = user2Id,
            Limit = limit,
            BeforeTimestamp = before
        };

        var response = await _chatService.LoadMessagesAsync(dto);
        return Ok(response);
    }

    // ---------------------------------------------------------
    // MARK DELIVERED
    // ---------------------------------------------------------
    [HttpPost("delivered")]
    public async Task<IActionResult> MarkDelivered([FromBody] MarkMessageDeliveredDto dto)
    {
        await _chatService.MarkDeliveredAsync(dto.ConversationId, dto.MessageId);
        return Ok();
    }

    // ---------------------------------------------------------
    // MARK SEEN
    // ---------------------------------------------------------
    [HttpPost("seen")]
    public async Task<IActionResult> MarkSeen([FromBody] MarkSeenDto dto)
    {
        var userId = User.GetUserId();
        await _chatService.MarkSeenAsync(dto.ConversationId, dto.MessageId, userId);
        return Ok();
    }

    // ---------------------------------------------------------
    // GET USER INBOX
    // ---------------------------------------------------------
    [HttpGet("inbox")]
    public async Task<ActionResult<List<ConversationPreviewDto>>> Inbox([FromQuery] int limit = 30)
    {
        var userId = User.GetUserId();

        var list = await _conversationService.GetInboxAsync(userId, limit);
        return Ok(list);
    }
}

