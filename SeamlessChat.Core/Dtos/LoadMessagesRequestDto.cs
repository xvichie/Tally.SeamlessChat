namespace SeamlessChat.Core.Dtos;

public class LoadMessagesRequestDto
{
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }

    public int Limit { get; set; } = 50;

    public long? BeforeTimestamp { get; set; }
}
