using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Auth;

public class ResendEmailVerificationRequest
{
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;
}
