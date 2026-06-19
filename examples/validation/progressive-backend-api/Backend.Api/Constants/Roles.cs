namespace Backend.Api.Constants;

public static class Roles
{
    public const string User = "User";

    public const string Admin = "Admin";

    public static readonly string[] All = [User, Admin];

    public static bool IsValid(string role)
    {
        return All.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public static string Normalize(string role)
    {
        return All.FirstOrDefault(
            value => value.Equals(role, StringComparison.OrdinalIgnoreCase)) ?? role;
    }
}
