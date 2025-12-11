using SeamlessChat.Core.Entities;

namespace SeamlessChat.Core.Interfaces;

public interface IMessageRepository
{
    Task AddMessageAsync(Message message);

    Task<List<Message>> GetMessagesAsync(
        string conversationId,
        int limit = 50,
        DateTime? before = null);

    Task MarkDeliveredAsync(string conversationId, Guid messageId, DateTime deliveredAt);

    Task MarkSeenAsync(string conversationId, Guid messageId, DateTime seenAt);
}
