using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AccountingApi.DTOs.Authentication;
using AccountingApi.Features.Authentication;
using System.Security.Claims;

namespace AccountingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator, ILogger<AuthController> logger) : ControllerBase
{

    /// <summary>
    /// Authenticate user and return JWT tokens
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        var result = await mediator.Send(new LoginCommand(request));
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponseDto<UserInfoDto>>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await mediator.Send(new RegisterCommand(request));
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await mediator.Send(new RefreshTokenCommand(request));
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponseDto<string>>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ApiResponseDto<string>
            {
                Success = false,
                Message = "User not authenticated.",
                Errors = ["Invalid token"]
            });
        }

        var result = await mediator.Send(new ChangePasswordCommand(userId, request));
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Logout user and invalidate refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponseDto<string>>> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ApiResponseDto<string>
            {
                Success = false,
                Message = "User not authenticated.",
                Errors = ["Invalid token"]
            });
        }

        var result = await mediator.Send(new LogoutCommand(userId));
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponseDto<UserInfoDto>>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ApiResponseDto<UserInfoDto>
            {
                Success = false,
                Message = "User not authenticated.",
                Errors = ["Invalid token"]
            });
        }

        var result = await mediator.Send(new GetCurrentUserQuery(userId));
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
