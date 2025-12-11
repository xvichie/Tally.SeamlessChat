using SeamlessChat.Core.Enums;
using SeamlessChat.Core.ValueObjects;

namespace SeamlessChat.Core.Interfaces;

public interface IMediaStorage
{
    Task<PresignedUploadResult> GeneratePresignedUploadAsync(
        string fileName,
        string contentType,
        MediaType mediaType);
}
