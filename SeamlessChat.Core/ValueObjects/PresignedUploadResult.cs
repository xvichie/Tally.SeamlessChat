namespace SeamlessChat.Core.ValueObjects;

public class PresignedUploadResult
{
    public string UploadUrl { get; }
    public string FinalUrl { get; }

    public DateTime ExpiresAt { get; }

    public PresignedUploadResult(string uploadUrl, string finalUrl, DateTime expiresAt)
    {
        UploadUrl = uploadUrl;
        FinalUrl = finalUrl;
        ExpiresAt = expiresAt;
    }
}

