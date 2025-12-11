using SeamlessChat.Core.Enums;

namespace SeamlessChat.Core.Dtos;

public class MediaAttachmentDto
{
    public string Url { get; set; } = default!;
    public MediaType MediaType { get; set; }
}

