namespace SeamlessChat.Core.ValueObjects;

public class Participant
{
    public Guid UserId { get; private set; }

    public Participant(Guid userId)
    {
        UserId = userId;
    }
}
