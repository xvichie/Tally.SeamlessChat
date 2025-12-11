using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using SeamlessChat.Core.Enums;
using SeamlessChat.Core.Interfaces;
using SeamlessChat.Core.Options;
using SeamlessChat.Core.ValueObjects;

namespace SeamlessChat.Infrastructure.Services;

public class S3MediaStorage : IMediaStorage
{
    private readonly IAmazonS3 _s3;
    private readonly ChatMediaOptions _options;

    public S3MediaStorage(
        IAmazonS3 s3,
        IOptions<ChatMediaOptions> options)
    {
        _s3 = s3;
        _options = options.Value;
    }

    public async Task<PresignedUploadResult> GeneratePresignedUploadAsync(
        string fileName,
        string contentType,
        MediaType mediaType)
    {
        // --------------------------------------------------------------------
        // 1) Create a unique file path inside bucket
        // --------------------------------------------------------------------
        var ext = Path.GetExtension(fileName);
        var newName = $"{Guid.NewGuid()}{ext}";

        var folder = mediaType switch
        {
            MediaType.Image => "images",
            MediaType.Video => "videos",
            MediaType.Audio => "audio",
            _ => "unknown"
        };

        var key = $"{_options.BaseFolder}/{folder}/{newName}";

        // --------------------------------------------------------------------
        // 2) Generate presigned upload URL
        // --------------------------------------------------------------------
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = expiresAt,
            ContentType = contentType
        };

        var presignedUrl = _s3.GetPreSignedURL(request);

        // --------------------------------------------------------------------
        // 3) Public GET URL (after upload)
        // --------------------------------------------------------------------
        // If your bucket is private, this URL will require signed access.
        // Host app can front it with CloudFront if needed.

        var finalUrl = $"https://{_options.BucketName}.s3.amazonaws.com/{key}";

        return new PresignedUploadResult(
            uploadUrl: presignedUrl,
            finalUrl: finalUrl,
            expiresAt: expiresAt
        );
    }
}

