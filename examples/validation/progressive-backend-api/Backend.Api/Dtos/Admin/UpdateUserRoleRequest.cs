using System.ComponentModel.DataAnnotations;
using Backend.Api.Constants;

namespace Backend.Api.Dtos.Admin;

public class UpdateUserRoleRequest : IValidatableObject
{
    [Required]
    public string Role { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Roles.IsValid(Role))
        {
            yield return new ValidationResult(
                "Role is invalid.",
                [nameof(Role)]);
        }
    }
}
