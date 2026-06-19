using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Admin;

public class UpdateUserStatusRequest
{
    [Required]
    public bool? IsActive { get; set; }
}
