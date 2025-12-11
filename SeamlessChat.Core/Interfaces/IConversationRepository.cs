using SeamlessChat.Core.Entities;

namespace SeamlessChat.Core.Interfaces;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(string conversationId);

    Task CreateIfNotExistsAsync(Conversation conversation);

    Task UpdateLastMessageAsync(
        string conversationId,
        string? lastMessageText,
        DateTime lastMessageAt);

    Task<List<Conversation>> GetUserInboxAsync(Guid userId, int limit);
}
