using AccountingApi.DTOs.Authentication;
using AccountingApi.Features.Authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingApi.Controllers;

using MyMediator;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Authenticate user and return JWT tokens.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _mediator.Send(new LoginCommand(request));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponseDto<UserInfoDto>>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _mediator.Send(new RegisterCommand(request));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>
    /// Refresh access token using refresh token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Change user password.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponseDto<string>>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new ApiResponseDto<string> { Success = false, Message = "User not authenticated.", Errors = ["Invalid token"] });

        var result = await _mediator.Send(new ChangePasswordCommand(userId, request));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Logout user and invalidate refresh token.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponseDto<string>>> Logout()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new ApiResponseDto<string> { Success = false, Message = "User not authenticated.", Errors = ["Invalid token"] });

        var result = await _mediator.Send(new LogoutCommand(userId));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get current user information.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponseDto<UserInfoDto>>> GetCurrentUser()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new ApiResponseDto<UserInfoDto> { Success = false, Message = "User not authenticated.", Errors = ["Invalid token"] });

        var result = await _mediator.Send(new GetCurrentUserQuery(userId));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update user profile information.
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponseDto<UserInfoDto>>> UpdateProfile([FromBody] UpdateUserProfileDto request)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new ApiResponseDto<UserInfoDto> { Success = false, Message = "User not authenticated.", Errors = ["Invalid token"] });

        var result = await _mediator.Send(new UpdateUserProfileCommand(userId, request));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}