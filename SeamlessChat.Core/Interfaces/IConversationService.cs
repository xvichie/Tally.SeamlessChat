using SeamlessChat.Core.Dtos;
using SeamlessChat.Core.Entities;

namespace SeamlessChat.Core.Interfaces;

public interface IConversationService
{
    string GetConversationId(Guid user1, Guid user2);

    Task<Conversation> GetOrCreateConversationAsync(Guid user1, Guid user2);

    Task<List<ConversationPreviewDto>> GetInboxAsync(Guid userId, int limit);

    Task UpdateConversationPreviewAsync(
        string conversationId,
        string? lastMessageText,
        DateTime lastMessageAt);
}
