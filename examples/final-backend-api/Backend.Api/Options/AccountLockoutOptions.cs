using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Options;

public class AccountLockoutOptions
{
    public const string SectionName = "AccountLockout";

    [Range(1, 20)]
    public int MaxFailedAccessAttempts { get; set; } = 5;

    [Range(1, 1440)]
    public int LockoutMinutes { get; set; } = 15;
}
