using AccountingApi.Features.Authentication;
using AccountingApi.Models;
using AccountingApi.Tests.TestHelpers;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Moq;

using static AccountingApi.Tests.TestHelpers.ApiResponseAssertions;

namespace AccountingApi.Tests.Features.Authentication;

public class LogoutHandlerTests
{
    private Mock<UserManager<ApplicationUser>> _userManager = null!;
    private Mock<ILogger<LogoutHandler>> _logger = null!;
    private LogoutHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userManager = IdentityMocks.CreateUserManager();
        _logger = new Mock<ILogger<LogoutHandler>>();
        _handler = new LogoutHandler(_userManager.Object, _logger.Object);
    }

    [Test]
    public async Task User_Not_Found_Still_Succeeds()
    {
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync((ApplicationUser?)null);
        var res = await _handler.Handle(new LogoutCommand("u1"), default);
        EnsureSuccess(res);
        _userManager.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Test]
    public async Task User_Found_Clears_Tokens_And_Updates()
    {
        var user = new ApplicationUser { Id = "u1", RefreshToken = "rt", RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(1) };
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        var res = await _handler.Handle(new LogoutCommand("u1"), default);
        EnsureSuccess(res);
        Assert.That(user.RefreshToken, Is.Null);
        Assert.That(user.RefreshTokenExpiryTime, Is.Null);
        _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
    }
}