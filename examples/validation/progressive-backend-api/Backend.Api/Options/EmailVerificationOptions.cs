using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Options;

public class EmailVerificationOptions
{
    public const string SectionName = "EmailVerification";

    [Range(1, 168)]
    public int ExpirationHours { get; set; } = 24;

    [Required]
    public string VerificationUrlTemplate { get; set; } =
        "http://localhost:3000/verify-email?token={token}";
}
