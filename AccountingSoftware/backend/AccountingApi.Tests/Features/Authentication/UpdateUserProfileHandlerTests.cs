using AccountingApi.DTOs.Authentication;
using AccountingApi.Features.Authentication;
using AccountingApi.Models;
using AccountingApi.Tests.TestHelpers;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Moq;

using static AccountingApi.Tests.TestHelpers.ApiResponseAssertions;

namespace AccountingApi.Tests.Features.Authentication;

public class UpdateUserProfileHandlerTests
{
    private Mock<UserManager<ApplicationUser>> _userManager = null!;
    private Mock<ILogger<UpdateUserProfileHandler>> _logger = null!;
    private UpdateUserProfileHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userManager = IdentityMocks.CreateUserManager();
        _logger = new Mock<ILogger<UpdateUserProfileHandler>>();
        _handler = new UpdateUserProfileHandler(_userManager.Object, _logger.Object);
    }

    [Test]
    public async Task User_Not_Found()
    {
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync((ApplicationUser?)null);
        var res = await _handler.Handle(new UpdateUserProfileCommand("u1", new UpdateUserProfileDto()), default);
        EnsureFailure(res);
        HasError(res, "user not found");
    }

    [Test]
    public async Task Email_Already_Exists()
    {
        var user = new ApplicationUser { Id = "u1", Email = "old@x.com" };
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
        _userManager.Setup(m => m.FindByEmailAsync("new@x.com")).ReturnsAsync(new ApplicationUser { Id = "u2" });
        var dto = new UpdateUserProfileDto { FirstName = "F", LastName = "L", Email = "new@x.com" };
        var res = await _handler.Handle(new UpdateUserProfileCommand("u1", dto), default);
        EnsureFailure(res);
        HasError(res, "email already exists");
    }

    [Test]
    public async Task Update_Fails()
    {
        var user = new ApplicationUser { Id = "u1", Email = "old@x.com" };
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
        _userManager.Setup(m => m.FindByEmailAsync("new@x.com")).ReturnsAsync((ApplicationUser?)null);
        _userManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "bad" }));
        var dto = new UpdateUserProfileDto { FirstName = "F", LastName = "L", Email = "new@x.com" };
        var res = await _handler.Handle(new UpdateUserProfileCommand("u1", dto), default);
        EnsureFailure(res);
        HasError(res, "bad");
    }

    [Test]
    public async Task Success_Returns_Updated_Info()
    {
        var user = new ApplicationUser { Id = "u1", Email = "old@x.com" };
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
        _userManager.Setup(m => m.FindByEmailAsync("new@x.com")).ReturnsAsync((ApplicationUser?)null);
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "User" });
        var dto = new UpdateUserProfileDto { FirstName = "F", LastName = "L", Email = "new@x.com" };
        var res = await _handler.Handle(new UpdateUserProfileCommand("u1", dto), default);
        var data = EnsureSuccess(res);
        Assert.That(data.Email, Is.EqualTo("new@x.com"));
        Assert.That(data.Roles.Contains("User"), Is.True);
    }
}