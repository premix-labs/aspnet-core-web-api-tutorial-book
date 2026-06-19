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
        if (role.Equals(Admin, StringComparison.OrdinalIgnoreCase))
        {
            return Admin;
        }

        if (role.Equals(User, StringComparison.OrdinalIgnoreCase))
        {
            return User;
        }

        return role;
    }
}
