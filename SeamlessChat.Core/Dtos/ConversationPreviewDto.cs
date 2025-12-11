namespace SeamlessChat.Core.Dtos;

public class ConversationPreviewDto
{
    public string ConversationId { get; set; } = default!;

    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }

    public string? LastMessageText { get; set; }
    public DateTime LastMessageAt { get; set; }
}
