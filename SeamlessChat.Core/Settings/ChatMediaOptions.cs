namespace SeamlessChat.Core.Settings;

public class S3BucketSettings
{
    public string BucketName { get; set; } = default!;
    public string BaseFolder { get; set; } = "chat";
    public int PresignExpirationMinutes { get; set; } = 10;
}
