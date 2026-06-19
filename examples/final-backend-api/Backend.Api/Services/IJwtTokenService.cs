using Backend.Api.Models;

namespace Backend.Api.Services;

public interface IJwtTokenService
{
    TokenResult CreateToken(User user);
}
