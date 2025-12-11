namespace SeamlessChat.Core.Dtos;

public class SendMessageDto
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }

    public string? Content { get; set; }

    public MediaAttachmentDto? Attachment { get; set; }
}
