using SeamlessChat.Core.ValueObjects;

namespace SeamlessChat.Core.Entities;

public class Conversation
{
    public string ConversationId { get; private set; }

    public Participant User1 { get; private set; }
    public Participant User2 { get; private set; }

    public string? LastMessageText { get; private set; }
    public DateTime LastMessageAt { get; private set; }

    public Conversation(
        Guid user1Id,
        Guid user2Id,
        DateTime createdAt)
    {
        if (user1Id == user2Id)
            throw new ArgumentException("Conversation participants must be different users.");

        // Deterministic stable ID for Dynamo partition key
        var ordered = new[] { user1Id, user2Id }
            .OrderBy(x => x.ToString())
            .ToArray();

        ConversationId = $"{ordered[0]}_{ordered[1]}";
        User1 = new Participant(ordered[0]);
        User2 = new Participant(ordered[1]);
        LastMessageAt = createdAt;
    }

    public void UpdateLastMessage(string? text, DateTime when)
    {
        LastMessageText = text;
        LastMessageAt = when;
    }

    public bool IsParticipant(Guid userId) =>
        User1.UserId == userId || User2.UserId == userId;
}
