using SeamlessChat.Core.Dtos;

namespace SeamlessChat.Core.Interfaces;

public interface IChatService
{
    Task<MessageDto> SendMessageAsync(SendMessageDto dto);

    Task<LoadMessagesResponseDto> LoadMessagesAsync(
        LoadMessagesRequestDto dto);

    Task MarkDeliveredAsync(string conversationId, Guid messageId);

    Task MarkSeenAsync(string conversationId, Guid messageId, Guid userId);
}
