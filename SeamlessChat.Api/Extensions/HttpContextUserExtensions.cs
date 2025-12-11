using System.Security.Claims;

namespace SeamlessChat.Api.Extensions;

public static class HttpContextUserExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? throw new UnauthorizedAccessException("User id missing");

        return Guid.Parse(id);
    }
}

