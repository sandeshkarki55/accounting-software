using System.Security.Claims;

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

public class RefreshTokenHandlerTests
{
    private Mock<UserManager<ApplicationUser>> _userManager = null!;
    private Mock<IJwtService> _jwt = null!;
    private Mock<ILogger<RefreshTokenHandler>> _logger = null!;
    private RefreshTokenHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userManager = IdentityMocks.CreateUserManager();
        _jwt = new Mock<IJwtService>();
        _logger = new Mock<ILogger<RefreshTokenHandler>>();
        _handler = new RefreshTokenHandler(_userManager.Object, _jwt.Object, _logger.Object);
    }

    [Test]
    public async Task Invalid_Access_Token_Principal_Null()
    {
        _jwt.Setup(j => j.GetPrincipalFromExpiredToken("bad")).Returns((ClaimsPrincipal?)null);
        var res = await _handler.Handle(new RefreshTokenCommand(new RefreshTokenRequestDto { AccessToken = "bad", RefreshToken = "r" }), default);
        EnsureFailure(res);
        HasError(res, "invalid token");
    }

    [Test]
    public async Task Missing_UserId_Claim()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        _jwt.Setup(j => j.GetPrincipalFromExpiredToken("x")).Returns(principal);
        var res = await _handler.Handle(new RefreshTokenCommand(new RefreshTokenRequestDto { AccessToken = "x", RefreshToken = "r" }), default);
        EnsureFailure(res);
        HasError(res, "invalid token payload");
    }

    [Test]
    public async Task User_Not_Found_Or_Token_Mismatch_Or_Expired()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "u1") }));
        _jwt.Setup(j => j.GetPrincipalFromExpiredToken("x")).Returns(principal);
        // user null
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync((ApplicationUser?)null);
        var res1 = await _handler.Handle(new RefreshTokenCommand(new RefreshTokenRequestDto { AccessToken = "x", RefreshToken = "r" }), default);
        EnsureFailure(res1);

        // mismatch
        var user = new ApplicationUser { Id = "u1", RefreshToken = "other", RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(1) };
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
        var res2 = await _handler.Handle(new RefreshTokenCommand(new RefreshTokenRequestDto { AccessToken = "x", RefreshToken = "r" }), default);
        EnsureFailure(res2);

        // expired
        user = new ApplicationUser { Id = "u1", RefreshToken = "r", RefreshTokenExpiryTime = DateTime.UtcNow.AddSeconds(-1) };
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
        var res3 = await _handler.Handle(new RefreshTokenCommand(new RefreshTokenRequestDto { AccessToken = "x", RefreshToken = "r" }), default);
        EnsureFailure(res3);
    }

    [Test]
    public async Task Success_New_Tokens_Saved()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "u1") }));
        _jwt.Setup(j => j.GetPrincipalFromExpiredToken("x")).Returns(principal);
        var user = new ApplicationUser { Id = "u1", Email = "e", FirstName = "F", LastName = "L", RefreshToken = "r", RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(1) };
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateAccessTokenAsync(user)).ReturnsAsync("new-at");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("new-rt");
        _jwt.Setup(j => j.SaveRefreshTokenAsync(user, "new-rt")).ReturnsAsync(true);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "User" });

        var res = await _handler.Handle(new RefreshTokenCommand(new RefreshTokenRequestDto { AccessToken = "x", RefreshToken = "r" }), default);
        var data = EnsureSuccess(res);
        Assert.That(data.AccessToken, Is.EqualTo("new-at"));
        Assert.That(data.RefreshToken, Is.EqualTo("new-rt"));
        Assert.That(data.User.Roles.Contains("User"), Is.True);
    }
}