using SeamlessChat.Core.Enums;

namespace SeamlessChat.Core.Dtos;

public class PresignMediaRequestDto
{
    public MediaType MediaType { get; set; }
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
}
