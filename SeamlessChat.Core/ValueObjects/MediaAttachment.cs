using SeamlessChat.Core.Enums;

namespace SeamlessChat.Core.ValueObjects;

public class MediaAttachment
{
    public string Url { get; private set; }
    public MediaType MediaType { get; private set; }

    public MediaAttachment(string url, MediaType mediaType)
    {
        Url = url;
        MediaType = mediaType;
    }
}
