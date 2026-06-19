using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Admin;

public class AdminUserQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    [StringLength(256)]
    public string? Search { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }

    public string SortBy { get; set; } = "createdAt";

    public string SortDirection { get; set; } = "desc";
}
