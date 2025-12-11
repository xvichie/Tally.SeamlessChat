namespace SeamlessChat.Core.Dtos;

public class MarkSeenDto
{
    public string ConversationId { get; set; } = default!;
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
}

