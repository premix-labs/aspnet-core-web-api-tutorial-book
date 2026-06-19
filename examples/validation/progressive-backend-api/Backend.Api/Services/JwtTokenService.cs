using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Backend.Api.Dtos.Auth;
using Backend.Api.Models;
using Backend.Api.Options;

namespace Backend.Api.Services;

public class JwtTokenService(IOptions<JwtOptions> jwtOptions)
{
    public LoginResponse GenerateLoginResponse(User user)
    {
        return GenerateLoginResponse(user, refreshToken: null);
    }

    public LoginResponse GenerateLoginResponse(
        User user,
        RefreshTokenResult? refreshToken)
    {
        var options = jwtOptions.Value;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(options.ExpirationMinutes);

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(options.SigningKey));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("role", user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new LoginResponse(
            accessToken,
            expiresAt,
            refreshToken?.Token ?? string.Empty,
            refreshToken?.ExpiresAt ?? DateTimeOffset.MinValue,
            new CurrentUserResponse(
                user.Id,
                user.Email,
                user.Role,
                user.IsActive,
                user.IsEmailVerified));
    }
}
