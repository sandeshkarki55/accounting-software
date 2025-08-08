using AccountingApi.DTOs.Authentication;
using AccountingApi.Features.Authentication;
using AccountingApi.Models;
using AccountingApi.Tests.TestHelpers;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Moq;

using static AccountingApi.Tests.TestHelpers.ApiResponseAssertions;

namespace AccountingApi.Tests.Features.Authentication;

public class ChangePasswordHandlerTests
{
    private Mock<UserManager<ApplicationUser>> _userManager = null!;
    private Mock<ILogger<ChangePasswordHandler>> _logger = null!;
    private ChangePasswordHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userManager = IdentityMocks.CreateUserManager();
        _logger = new Mock<ILogger<ChangePasswordHandler>>();
        _handler = new ChangePasswordHandler(_userManager.Object, _logger.Object);
    }

    [Test]
    public async Task User_Not_Found()
    {
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync((ApplicationUser?)null);
        var res = await _handler.Handle(new ChangePasswordCommand("u1", new ChangePasswordRequestDto()), default);
        EnsureFailure(res);
        HasError(res, "user not found");
    }

    [Test]
    public async Task Change_Fails()
    {
        var user = new ApplicationUser { Id = "u1" };
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
        _userManager.Setup(m => m.ChangePasswordAsync(user, "c", "n"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "bad" }));
        var res = await _handler.Handle(new ChangePasswordCommand("u1", new ChangePasswordRequestDto { CurrentPassword = "c", NewPassword = "n" }), default);
        EnsureFailure(res);
        HasError(res, "bad");
    }

    [Test]
    public async Task Success()
    {
        var user = new ApplicationUser { Id = "u1" };
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
        _userManager.Setup(m => m.ChangePasswordAsync(user, "c", "n")).ReturnsAsync(IdentityResult.Success);
        var res = await _handler.Handle(new ChangePasswordCommand("u1", new ChangePasswordRequestDto { CurrentPassword = "c", NewPassword = "n" }), default);
        var data = EnsureSuccess(res);
        Assert.That(data, Is.EqualTo("Password updated"));
    }
}