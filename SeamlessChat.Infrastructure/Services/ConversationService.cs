using SeamlessChat.Core.Dtos;
using SeamlessChat.Core.Entities;
using SeamlessChat.Core.Interfaces;

namespace SeamlessChat.Core.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;

    public ConversationService(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public string GetConversationId(Guid user1, Guid user2)
    {
        if (user1 == user2)
            throw new ArgumentException("Conversation participants must differ.");

        // Sort user IDs alphabetically to ensure stable ID
        var ordered = new[] { user1, user2 }
            .OrderBy(x => x.ToString())
            .ToArray();

        return $"{ordered[0]}_{ordered[1]}";
    }

    public async Task<Conversation> GetOrCreateConversationAsync(Guid user1, Guid user2)
    {
        var conversationId = GetConversationId(user1, user2);

        var existing = await _conversationRepository.GetByIdAsync(conversationId);
        if (existing != null)
            return existing;

        var created = new Conversation(user1, user2, DateTime.UtcNow);

        await _conversationRepository.CreateIfNotExistsAsync(created);

        return created;
    }
    public async Task UpdateConversationPreviewAsync(
        string conversationId,
        string? lastMessageText,
        DateTime lastMessageAt)
    {
        await _conversationRepository.UpdateLastMessageAsync(
            conversationId,
            lastMessageText,
            lastMessageAt
        );
    }

    public async Task<List<ConversationPreviewDto>> GetInboxAsync(Guid userId, int limit)
    {
        var items = await _conversationRepository.GetUserInboxAsync(userId, limit);

        return items
            .Select(c => new ConversationPreviewDto
            {
                ConversationId = c.ConversationId,
                User1Id = c.User1.UserId,
                User2Id = c.User2.UserId,
                LastMessageText = c.LastMessageText,
                LastMessageAt = c.LastMessageAt
            })
            .ToList();
    }

}
