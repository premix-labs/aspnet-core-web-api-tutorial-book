using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Api.Constants;
using Backend.Api.Dtos.Admin;
using Backend.Api.Dtos.Common;
using Backend.Api.Services;

namespace Backend.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[ApiController]
[Route("api/v1/admin/users")]
public class AdminUsersController(AdminUserService adminUserService) : ControllerBase
{
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public IActionResult Ping()
    {
        return Ok(new { message = "Admin endpoint is working" });
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<AdminUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers([FromQuery] AdminUserQuery query)
    {
        var users = await adminUserService.GetUsersAsync(query);

        return Ok(users);
    }

    [HttpPatch("{id:guid}/role")]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(
        Guid id,
        UpdateUserRoleRequest request)
    {
        var user = await adminUserService.UpdateRoleAsync(id, request);

        return Ok(user);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        UpdateUserStatusRequest request)
    {
        var user = await adminUserService.UpdateStatusAsync(id, request);

        return Ok(user);
    }
}
