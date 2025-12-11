using SeamlessChat.Core.Enums;
using SeamlessChat.Core.ValueObjects;

namespace SeamlessChat.Core.Entities;

public class Message
{
    public Guid MessageId { get; private set; }
    public string ConversationId { get; private set; }

    public Guid SenderId { get; private set; }
    public Guid ReceiverId { get; private set; }

    public string? Content { get; private set; }
    public MediaAttachment? Attachment { get; private set; }

    public MessageStatus Status { get; private set; }

    public DateTime SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? SeenAt { get; private set; }

    public Message(
        string conversationId,
        Guid senderId,
        Guid receiverId,
        string? content,
        MediaAttachment? attachment,
        DateTime sentAt,
        Guid? messageId = null)
    {
        MessageId = messageId ?? Guid.NewGuid();
        ConversationId = conversationId;

        SenderId = senderId;
        ReceiverId = receiverId;

        Content = content;
        Attachment = attachment;

        SentAt = sentAt;
        Status = MessageStatus.Sent;
    }

    public void MarkDelivered(DateTime when)
    {
        Status = MessageStatus.Delivered;
        DeliveredAt = when;
    }

    public void MarkSeen(DateTime when)
    {
        Status = MessageStatus.Seen;
        SeenAt = when;
    }
}
