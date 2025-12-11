namespace SeamlessChat.Core.Dtos;

public class LoadMessagesResponseDto
{
    public List<MessageDto> Messages { get; set; } = new();

    public long? NextCursor { get; set; }

    public bool HasMore { get; set; }
}
