using AccountingApi.Features.Authentication;
using AccountingApi.Models;
using AccountingApi.Tests.TestHelpers;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Moq;

using static AccountingApi.Tests.TestHelpers.ApiResponseAssertions;

namespace AccountingApi.Tests.Features.Authentication;

public class GetCurrentUserHandlerTests
{
    private Mock<UserManager<ApplicationUser>> _userManager = null!;
    private Mock<ILogger<GetCurrentUserHandler>> _logger = null!;
    private GetCurrentUserHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userManager = IdentityMocks.CreateUserManager();
        _logger = new Mock<ILogger<GetCurrentUserHandler>>();
        _handler = new GetCurrentUserHandler(_userManager.Object, _logger.Object);
    }

    [Test]
    public async Task User_Not_Found()
    {
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync((ApplicationUser?)null);
        var res = await _handler.Handle(new GetCurrentUserQuery("u1"), default);
        EnsureFailure(res);
        HasError(res, "user not found");
    }

    [Test]
    public async Task Success_Returns_Info()
    {
        var user = new ApplicationUser { Id = "u1", Email = "e", FirstName = "F", LastName = "L" };
        _userManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "User" });
        var res = await _handler.Handle(new GetCurrentUserQuery("u1"), default);
        var data = EnsureSuccess(res);
        Assert.That(data.Id, Is.EqualTo("u1"));
        Assert.That(data.Roles.Contains("User"), Is.True);
    }
}