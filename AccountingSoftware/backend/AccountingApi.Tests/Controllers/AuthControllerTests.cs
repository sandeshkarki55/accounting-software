using System.Security.Claims;

using AccountingApi.Controllers;
using AccountingApi.DTOs.Authentication;
using AccountingApi.Features.Authentication;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using MyMediator;

using static AccountingApi.Tests.TestHelpers.ApiResponseAssertions;
using static AccountingApi.Tests.TestHelpers.ResultAssertions;

namespace AccountingApi.Tests.Controllers;

public class AuthControllerTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
        _controller = new AuthController(_mediatorMock.Object);
    }

    private static ApiResponseDto<T> Success<T>(T data, string message = "ok") => new ApiResponseDto<T>
    {
        Success = true,
        Message = message,
        Data = data
    };

    private static ApiResponseDto<T> Failure<T>(string message = "err", params string[] errors) => new ApiResponseDto<T>
    {
        Success = false,
        Message = message,
        Errors = errors.ToList()
    };

    [Test]
    public async Task Login_ReturnsOk_WhenSuccess()
    {
        var req = new LoginRequestDto { Email = "a@b.com", Password = "x" };
        var expected = Success(new LoginResponseDto { AccessToken = "a", RefreshToken = "r" }, "Login successful.");
        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Login(req);
        var payload = Ok(result);
        var data = EnsureSuccess(payload);
        Assert.That(data.AccessToken, Is.EqualTo("a"));
        _mediatorMock.Verify(m => m.Send(It.Is<LoginCommand>(c => c.LoginRequest == req), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Login_ReturnsBadRequest_WhenFailure()
    {
        var req = new LoginRequestDto { Email = "bad@b.com", Password = "x" };
        var expected = Failure<LoginResponseDto>("Login failed.", "Invalid credentials");
        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Login(req);
        var payload = BadRequest<LoginResponseDto>(result);
        EnsureFailure(payload);
        _mediatorMock.Verify(m => m.Send(It.Is<LoginCommand>(c => c.LoginRequest == req), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Register_ReturnsCreated_WhenSuccess()
    {
        var req = new RegisterRequestDto { Email = "a@b.com" };
        var expected = Success(new UserInfoDto { Id = "u1" }, "Registration successful.");
        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Register(req);
        var payload = Created<UserInfoDto>(result);
        var data = EnsureSuccess(payload);
        Assert.That(data.Id, Is.EqualTo("u1"));
        _mediatorMock.Verify(m => m.Send(It.Is<RegisterCommand>(c => c.RegisterRequest == req), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Register_ReturnsBadRequest_WhenFailure()
    {
        var req = new RegisterRequestDto { Email = "a@b.com" };
        var expected = Failure<UserInfoDto>("User exists", "Email already registered");
        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Register(req);
        var payload = BadRequest<UserInfoDto>(result);
        EnsureFailure(payload);
        _mediatorMock.Verify(m => m.Send(It.Is<RegisterCommand>(c => c.RegisterRequest == req), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RefreshToken_ReturnsOk_WhenSuccess()
    {
        var req = new RefreshTokenRequestDto { AccessToken = "old", RefreshToken = "r1" };
        var expected = Success(new LoginResponseDto { AccessToken = "new" }, "Token refreshed successfully.");
        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.RefreshToken(req);
        var payload = Ok(result);
        var data = EnsureSuccess(payload);
        Assert.That(data.AccessToken, Is.EqualTo("new"));
        _mediatorMock.Verify(m => m.Send(It.Is<RefreshTokenCommand>(c => c.RefreshTokenRequest == req), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RefreshToken_ReturnsBadRequest_WhenFailure()
    {
        var req = new RefreshTokenRequestDto { AccessToken = "bad", RefreshToken = "bad" };
        var expected = Failure<LoginResponseDto>("Invalid refresh token", "Invalid or expired refresh token");
        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.RefreshToken(req);
        var payload = BadRequest<LoginResponseDto>(result);
        EnsureFailure(payload);
        _mediatorMock.Verify(m => m.Send(It.Is<RefreshTokenCommand>(c => c.RefreshTokenRequest == req), It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetUser(string? id)
    {
        var claims = new List<Claim>();
        if (!string.IsNullOrEmpty(id))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, id));
        }
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Test]
    public async Task ChangePassword_ReturnsUnauthorized_WhenNoUser()
    {
        SetUser(null);
        var result = await _controller.ChangePassword(new ChangePasswordRequestDto());
        var payload = Unauthorized<string>(result);
        EnsureFailure(payload);
    }

    [Test]
    public async Task ChangePassword_ReturnsOk_WhenSuccess()
    {
        SetUser("user-1");
        var expected = Success("Password updated", "Password changed successfully.");
        _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.ChangePassword(new ChangePasswordRequestDto { CurrentPassword = "c", NewPassword = "n" });
        var payload = Ok(result);
        EnsureSuccess(payload);
        _mediatorMock.Verify(m => m.Send(It.Is<ChangePasswordCommand>(c => c.UserId == "user-1"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ChangePassword_ReturnsBadRequest_WhenFailure()
    {
        SetUser("user-1");
        var expected = Failure<string>("Failed to change password.", "error");
        _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.ChangePassword(new ChangePasswordRequestDto { CurrentPassword = "c", NewPassword = "n" });
        var payload = BadRequest<string>(result);
        EnsureFailure(payload);
    }

    [Test]
    public async Task Logout_ReturnsUnauthorized_WhenNoUser()
    {
        SetUser(null);
        var result = await _controller.Logout();
        var payload = Unauthorized<string>(result);
        EnsureFailure(payload);
    }

    [Test]
    public async Task Logout_ReturnsOk_WhenSuccess()
    {
        SetUser("user-1");
        var expected = Success("Logged out", "Logout successful.");
        _mediatorMock.Setup(m => m.Send(It.IsAny<LogoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Logout();
        var payload = Ok(result);
        EnsureSuccess(payload);
        _mediatorMock.Verify(m => m.Send(It.Is<LogoutCommand>(c => c.UserId == "user-1"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetCurrentUser_ReturnsUnauthorized_WhenNoUser()
    {
        SetUser(null);
        var result = await _controller.GetCurrentUser();
        var payload = Unauthorized<UserInfoDto>(result);
        EnsureFailure(payload);
    }

    [Test]
    public async Task GetCurrentUser_ReturnsOk_WhenSuccess()
    {
        SetUser("user-1");
        var expected = Success(new UserInfoDto { Id = "user-1" }, "User information retrieved successfully.");
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetCurrentUser();
        var payload = Ok(result);
        var data = EnsureSuccess(payload);
        Assert.That(data.Id, Is.EqualTo("user-1"));
        _mediatorMock.Verify(m => m.Send(It.Is<GetCurrentUserQuery>(q => q.UserId == "user-1"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetCurrentUser_ReturnsBadRequest_WhenFailure()
    {
        SetUser("user-1");
        var expected = Failure<UserInfoDto>("User not found.", "User not found");
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetCurrentUser();
        var payload = BadRequest<UserInfoDto>(result);
        EnsureFailure(payload);
    }

    [Test]
    public async Task UpdateProfile_ReturnsUnauthorized_WhenNoUser()
    {
        SetUser(null);
        var result = await _controller.UpdateProfile(new UpdateUserProfileDto());
        var payload = Unauthorized<UserInfoDto>(result);
        EnsureFailure(payload);
    }

    [Test]
    public async Task UpdateProfile_ReturnsOk_WhenSuccess()
    {
        SetUser("user-1");
        var expected = Success(new UserInfoDto { Id = "user-1", Email = "e@x.com" }, "Profile updated successfully.");
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var dto = new UpdateUserProfileDto { FirstName = "F", LastName = "L", Email = "e@x.com" };
        var result = await _controller.UpdateProfile(dto);
        var payload = Ok(result);
        var data = EnsureSuccess(payload);
        Assert.That(data.Email, Is.EqualTo("e@x.com"));
        _mediatorMock.Verify(m => m.Send(It.Is<UpdateUserProfileCommand>(c => c.UserId == "user-1" && c.UpdateProfileRequest == dto), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateProfile_ReturnsBadRequest_WhenFailure()
    {
        SetUser("user-1");
        var expected = Failure<UserInfoDto>("Failed to update user profile.", "error");
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.UpdateProfile(new UpdateUserProfileDto());
        var payload = BadRequest<UserInfoDto>(result);
        EnsureFailure(payload);
    }
}