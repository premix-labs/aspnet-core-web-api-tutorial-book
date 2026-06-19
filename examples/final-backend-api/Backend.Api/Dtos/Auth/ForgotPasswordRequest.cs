using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Auth;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;
}
