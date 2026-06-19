namespace Backend.Api.Dtos.Users;

using System.ComponentModel.DataAnnotations;

public class UpdateUserRequest : IValidatableObject
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Email.EndsWith("@example.invalid", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Email domain is not allowed.",
                [nameof(Email)]);
        }
    }
}
