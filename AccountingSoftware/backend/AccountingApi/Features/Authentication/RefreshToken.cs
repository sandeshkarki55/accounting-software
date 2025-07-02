using MediatR;
using Microsoft.AspNetCore.Identity;
using AccountingApi.DTOs.Authentication;
using AccountingApi.Models;
using AccountingApi.Services;
using System.Security.Claims;

namespace AccountingApi.Features.Authentication;

// Command to handle token refresh
public record RefreshTokenCommand(RefreshTokenRequestDto RefreshTokenRequest) : IRequest<ApiResponseDto<LoginResponseDto>>;

// Handler for RefreshToken
public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, ApiResponseDto<LoginResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        ILogger<RefreshTokenHandler> logger)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<ApiResponseDto<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.RefreshTokenRequest.AccessToken);
        if (principal == null)
        {
            return new ApiResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "Invalid access token.",
                Errors = ["Invalid token"]
            };
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return new ApiResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "Invalid token payload.",
                Errors = ["Invalid token payload"]
            };
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.RefreshToken != request.RefreshTokenRequest.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return new ApiResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "Invalid refresh token.",
                Errors = ["Invalid or expired refresh token"]
            };
        }

        // Generate new tokens
        var newAccessToken = await _jwtService.GenerateAccessTokenAsync(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        
        // Save new refresh token
        await _jwtService.SaveRefreshTokenAsync(user, newRefreshToken);

        var userRoles = await _userManager.GetRolesAsync(user);

        var response = new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600,
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Roles = userRoles.ToList()
            }
        };

        _logger.LogInformation("Token refreshed successfully for user {UserId}", userId);

        return new ApiResponseDto<LoginResponseDto>
        {
            Success = true,
            Message = "Token refreshed successfully.",
            Data = response
        };
    }
}
