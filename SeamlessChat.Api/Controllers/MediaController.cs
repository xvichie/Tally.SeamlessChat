using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeamlessChat.Core.Dtos;
using SeamlessChat.Core.Interfaces;
using SeamlessChat.Core.ValueObjects;

namespace SeamlessChat.Api.Controllers;

[ApiController]
[Route("chat/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMediaStorage _mediaStorage;

    public MediaController(IMediaStorage mediaStorage)
    {
        _mediaStorage = mediaStorage;
    }

    [HttpPost("presign")]
    public async Task<ActionResult<PresignedUploadResult>> GetPresignedUrl([FromBody] PresignMediaRequestDto dto)
    {
        var result = await _mediaStorage.GeneratePresignedUploadAsync(
            dto.FileName,
            dto.ContentType,
            dto.MediaType);

        return Ok(result);
    }
}

