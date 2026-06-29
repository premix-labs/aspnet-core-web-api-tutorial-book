namespace Backend.Api.Dtos.Users;

using System.ComponentModel.DataAnnotations;

public class CreateUserRequest : IValidatableObject
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Email) &&
            Email.EndsWith("@example.invalid", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Email domain is not allowed.",
                [nameof(Email)]);
        }
    }
}
