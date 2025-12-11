using SeamlessChat.Core.Enums;

namespace SeamlessChat.Core.Dtos;

public class MessageDto
{
    public Guid MessageId { get; set; }
    public string ConversationId { get; set; } = default!;

    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }

    public string? Content { get; set; }

    public MediaAttachmentDto? Attachment { get; set; }

    public MessageStatus Status { get; set; }

    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? SeenAt { get; set; }
}
