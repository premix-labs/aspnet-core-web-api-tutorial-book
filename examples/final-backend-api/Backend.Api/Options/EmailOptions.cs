using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    [Required]
    [EmailAddress]
    public string FromAddress { get; set; } = "no-reply@example.com";

    [Required]
    public string FromName { get; set; } = "Backend API";

    public bool UseSmtp { get; set; }

    public string? SmtpHost { get; set; }

    [Range(1, 65535)]
    public int SmtpPort { get; set; } = 587;

    public bool SmtpEnableSsl { get; set; } = true;

    public string? SmtpUsername { get; set; }

    public string? SmtpPassword { get; set; }
}
