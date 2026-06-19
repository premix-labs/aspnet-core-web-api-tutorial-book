namespace Backend.Api.Dtos.Admin;

public class AdminUserQuery
{
    public string? Search { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string SortBy { get; set; } = "createdAt";

    public string SortDirection { get; set; } = "desc";
}
