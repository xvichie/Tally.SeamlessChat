namespace SeamlessChat.Core.Dtos;

public class TypingDto
{
    public string ConversationId { get; set; } = default!;
    public Guid UserId { get; set; }
}

