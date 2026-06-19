using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Backend.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value =
                User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                User?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public string? Email =>
        User?.FindFirstValue(JwtRegisteredClaimNames.Email) ??
        User?.FindFirstValue(ClaimTypes.Email);

    public string? Role =>
        User?.FindFirstValue("role") ??
        User?.FindFirstValue(ClaimTypes.Role);
}
