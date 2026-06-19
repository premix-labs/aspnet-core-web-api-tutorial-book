using Backend.Api.Constants;

namespace Backend.Api.Tests;

public class RolesTests
{
    [Theory]
    [InlineData(Roles.User)]
    [InlineData(Roles.Admin)]
    [InlineData("user")]
    [InlineData("admin")]
    public void IsValid_WhenRoleIsKnown_ReturnsTrue(string role)
    {
        var result = Roles.IsValid(role);

        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("SuperAdmin")]
    [InlineData("Manager")]
    public void IsValid_WhenRoleIsUnknown_ReturnsFalse(string role)
    {
        var result = Roles.IsValid(role);

        Assert.False(result);
    }
}
