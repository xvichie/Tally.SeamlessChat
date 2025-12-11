namespace SeamlessChat.Core.Dtos;

public class MarkMessageDeliveredDto
{
    public string ConversationId { get; set; } = default!;
    public Guid MessageId { get; set; }
}
