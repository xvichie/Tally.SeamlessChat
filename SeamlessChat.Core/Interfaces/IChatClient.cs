using SeamlessChat.Core.Dtos;

namespace SeamlessChat.Core.Interfaces;

public interface IChatClient
{
    Task ReceiveMessage(MessageDto message);
    Task TypingStarted(Guid userId, string conversationId);
    Task MessageDelivered(Guid messageId, string conversationId);
    Task MessageSeen(Guid messageId, string conversationId);
}
