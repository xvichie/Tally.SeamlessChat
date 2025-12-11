namespace SeamlessChat.Core.Services;

using SeamlessChat.Core.Dtos;
using SeamlessChat.Core.Entities;
using SeamlessChat.Core.Interfaces;
using SeamlessChat.Core.ValueObjects;

public class ChatService : IChatService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IConversationService _conversationService;

    public ChatService(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IConversationService conversationService)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _conversationService = conversationService;
    }

    public async Task<MessageDto> SendMessageAsync(SendMessageDto dto)
    {
        var conversationId =
            _conversationService.GetConversationId(dto.SenderId, dto.ReceiverId);

        // Ensure conversation record exists
        var conversation =
            await _conversationService.GetOrCreateConversationAsync(dto.SenderId, dto.ReceiverId);

        // Build domain Message
        var message = new Message(
            conversationId: conversationId,
            senderId: dto.SenderId,
            receiverId: dto.ReceiverId,
            content: dto.Content,
            attachment: dto.Attachment != null
                ? new MediaAttachment(dto.Attachment.Url, dto.Attachment.MediaType)
                : null,
            sentAt: DateTime.UtcNow
        );

        // Store in Dynamo
        await _messageRepository.AddMessageAsync(message);

        // Update conversation preview
        await _conversationRepository.UpdateLastMessageAsync(
            conversationId,
            dto.Content ?? "[media]",
            message.SentAt
        );

        // Return DTO
        return MapToDto(message);
    }

    public async Task<LoadMessagesResponseDto> LoadMessagesAsync(LoadMessagesRequestDto dto)
    {
        var conversationId = _conversationService.GetConversationId(dto.User1Id, dto.User2Id);

        var messages = await _messageRepository.GetMessagesAsync(
            conversationId,
            dto.Limit,
            dto.BeforeTimestamp.HasValue
                ? new DateTime(dto.BeforeTimestamp.Value, DateTimeKind.Utc)
                : null
        );

        // Response object
        var response = new LoadMessagesResponseDto
        {
            Messages = messages.Select(MapToDto).ToList()
        };

        if (messages.Count == dto.Limit)
        {
            // Next cursor = timestamp of last message
            response.NextCursor = messages.Last().SentAt.Ticks;
            response.HasMore = true;
        }
        else
        {
            response.NextCursor = null;
            response.HasMore = false;
        }

        return response;
    }
    public async Task MarkDeliveredAsync(string conversationId, Guid messageId)
    {
        await _messageRepository.MarkDeliveredAsync(
            conversationId,
            messageId,
            DateTime.UtcNow
        );
    }
    public async Task MarkSeenAsync(string conversationId, Guid messageId, Guid userId)
    {
        // Domain rules could be added here (optional)
        await _messageRepository.MarkSeenAsync(
            conversationId,
            messageId,
            DateTime.UtcNow
        );
    }

    private static MessageDto MapToDto(Message m)
    {
        return new MessageDto
        {
            MessageId = m.MessageId,
            ConversationId = m.ConversationId,
            SenderId = m.SenderId,
            ReceiverId = m.ReceiverId,
            Content = m.Content,
            Attachment = m.Attachment != null
                ? new MediaAttachmentDto
                {
                    Url = m.Attachment.Url,
                    MediaType = m.Attachment.MediaType
                }
                : null,
            Status = m.Status,
            SentAt = m.SentAt,
            DeliveredAt = m.DeliveredAt,
            SeenAt = m.SeenAt
        };
    }
}

