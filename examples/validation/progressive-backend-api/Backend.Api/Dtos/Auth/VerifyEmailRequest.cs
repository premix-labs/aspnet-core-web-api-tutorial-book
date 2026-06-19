using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Auth;

public class VerifyEmailRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}
