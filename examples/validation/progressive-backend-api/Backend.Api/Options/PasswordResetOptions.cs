using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Options;

public class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";

    [Range(1, 24)]
    public int ExpirationHours { get; set; } = 1;

    [Required]
    public string ResetUrlTemplate { get; set; } =
        "http://localhost:3000/reset-password?token={token}";
}
