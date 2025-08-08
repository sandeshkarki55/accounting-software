using AccountingApi.DTOs.Authentication;
using AccountingApi.Features.Authentication;
using AccountingApi.Models;
using AccountingApi.Tests.TestHelpers;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Moq;

using static AccountingApi.Tests.TestHelpers.ApiResponseAssertions;

namespace AccountingApi.Tests.Features.Authentication;

public class RegisterHandlerTests
{
    private Mock<UserManager<ApplicationUser>> _userManager = null!;
    private Mock<ILogger<RegisterHandler>> _logger = null!;
    private RegisterHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userManager = IdentityMocks.CreateUserManager();
        _logger = new Mock<ILogger<RegisterHandler>>();
        _handler = new RegisterHandler(_userManager.Object, _logger.Object);
    }

    [Test]
    public async Task Email_Already_Exists()
    {
        _userManager.Setup(m => m.FindByEmailAsync("e")).ReturnsAsync(new ApplicationUser());
        var res = await _handler.Handle(new RegisterCommand(new RegisterRequestDto { Email = "e", Password = "p" }), default);
        EnsureFailure(res);
        HasError(res, "already registered");
    }

    [Test]
    public async Task Create_Fails()
    {
        _userManager.Setup(m => m.FindByEmailAsync("e")).ReturnsAsync((ApplicationUser?)null);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "p"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "bad" }));
        var res = await _handler.Handle(new RegisterCommand(new RegisterRequestDto { Email = "e", Password = "p", FirstName = "F", LastName = "L" }), default);
        EnsureFailure(res);
        HasError(res, "bad");
    }

    [Test]
    public async Task Success_Assigns_Default_Role()
    {
        _userManager.Setup(m => m.FindByEmailAsync("e")).ReturnsAsync((ApplicationUser?)null);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "p"))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        var res = await _handler.Handle(new RegisterCommand(new RegisterRequestDto { Email = "e", Password = "p", FirstName = "F", LastName = "L" }), default);
        var data = EnsureSuccess(res);
        Assert.That(data.Roles.Contains("User"), Is.True);
    }
}