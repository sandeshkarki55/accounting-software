using AccountingApi.DTOs.Authentication;
using AccountingApi.Features.Authentication;
using AccountingApi.Models;
using AccountingApi.Services.JwtService;
using AccountingApi.Tests.TestHelpers;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Moq;

using static AccountingApi.Tests.TestHelpers.ApiResponseAssertions;

namespace AccountingApi.Tests.Features.Authentication;

public class LoginHandlerTests
{
    private Mock<UserManager<ApplicationUser>> _userManager = null!;
    private Mock<SignInManager<ApplicationUser>> _signInManager = null!;
    private Mock<IJwtService> _jwt = null!;
    private Mock<ILogger<LoginHandler>> _logger = null!;
    private LoginHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userManager = IdentityMocks.CreateUserManager();
        _signInManager = IdentityMocks.CreateSignInManager(_userManager.Object);
        _jwt = new Mock<IJwtService>();
        _logger = new Mock<ILogger<LoginHandler>>();
        _handler = new LoginHandler(_userManager.Object, _signInManager.Object, _jwt.Object, _logger.Object);
    }

    [Test]
    public async Task User_Not_Found()
    {
        _userManager.Setup(m => m.FindByEmailAsync("a@b.com")).ReturnsAsync((ApplicationUser?)null);
        var res = await _handler.Handle(new LoginCommand(new LoginRequestDto { Email = "a@b.com", Password = "x" }), default);
        EnsureFailure(res);
    }

    [Test]
    public async Task User_Inactive()
    {
        var user = new ApplicationUser { Email = "a@b.com", IsActive = false };
        _userManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        var res = await _handler.Handle(new LoginCommand(new LoginRequestDto { Email = user.Email!, Password = "x" }), default);
        EnsureFailure(res);
        HasError(res, "deactivated");
    }

    [Test]
    public async Task Password_Fail_LockedOut()
    {
        var user = new ApplicationUser { Email = "e", IsActive = true };
        _userManager.Setup(m => m.FindByEmailAsync("e")).ReturnsAsync(user);
        _signInManager.Setup(s => s.CheckPasswordSignInAsync(user, "x", true))
            .ReturnsAsync(SignInResult.LockedOut);
        var res = await _handler.Handle(new LoginCommand(new LoginRequestDto { Email = "e", Password = "x" }), default);
        EnsureFailure(res);
        HasError(res, "locked out");
    }

    [Test]
    public async Task Password_Fail_NotAllowed()
    {
        var user = new ApplicationUser { Email = "e", IsActive = true };
        _userManager.Setup(m => m.FindByEmailAsync("e")).ReturnsAsync(user);
        _signInManager.Setup(s => s.CheckPasswordSignInAsync(user, "x", true))
            .ReturnsAsync(SignInResult.NotAllowed);
        var res = await _handler.Handle(new LoginCommand(new LoginRequestDto { Email = "e", Password = "x" }), default);
        EnsureFailure(res);
        HasError(res, "not allowed");
    }

    [Test]
    public async Task Password_Fail_Generic()
    {
        var user = new ApplicationUser { Email = "e", IsActive = true };
        _userManager.Setup(m => m.FindByEmailAsync("e")).ReturnsAsync(user);
        _signInManager.Setup(s => s.CheckPasswordSignInAsync(user, "x", true))
            .ReturnsAsync(SignInResult.Failed);
        var res = await _handler.Handle(new LoginCommand(new LoginRequestDto { Email = "e", Password = "x" }), default);
        EnsureFailure(res);
        HasError(res, "invalid email or password");
    }

    [Test]
    public async Task Success_Generates_Tokens_And_Roles()
    {
        var user = new ApplicationUser { Id = "u1", Email = "e", FirstName = "F", LastName = "L", IsActive = true };
        _userManager.Setup(m => m.FindByEmailAsync("e")).ReturnsAsync(user);
        _signInManager.Setup(s => s.CheckPasswordSignInAsync(user, "x", true))
            .ReturnsAsync(SignInResult.Success);
        _jwt.Setup(j => j.GenerateAccessTokenAsync(user)).ReturnsAsync("at");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("rt");
        _jwt.Setup(j => j.SaveRefreshTokenAsync(user, "rt")).ReturnsAsync(true);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "User", "Admin" });

        var res = await _handler.Handle(new LoginCommand(new LoginRequestDto { Email = "e", Password = "x" }), default);
        var data = EnsureSuccess(res);
        Assert.That(data.AccessToken, Is.EqualTo("at"));
        Assert.That(data.RefreshToken, Is.EqualTo("rt"));
        Assert.That(data.User.Roles.Count, Is.EqualTo(2));
    }
}