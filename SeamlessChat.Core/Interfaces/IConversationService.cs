using SeamlessChat.Core.Entities;

namespace SeamlessChat.Core.Interfaces;

public interface IConversationService
{
    string GetConversationId(Guid user1, Guid user2);

    Task<Conversation> GetOrCreateConversationAsync(Guid user1, Guid user2);

    Task UpdateConversationPreviewAsync(
        string conversationId,
        string? lastMessageText,
        DateTime lastMessageAt);
}
