namespace SeamlessChat.Core.ValueObjects;

public class PresignedUploadResult
{
    public string UploadUrl { get; }
    public string FileUrl { get; }

    public PresignedUploadResult(string uploadUrl, string fileUrl)
    {
        UploadUrl = uploadUrl;
        FileUrl = fileUrl;
    }
}
