using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Options;

public class RefreshTokenOptions
{
    public const string SectionName = "RefreshTokens";

    [Range(1, 365)]
    public int ExpirationDays { get; set; } = 30;
}
