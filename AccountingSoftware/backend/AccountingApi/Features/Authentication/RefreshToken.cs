using Microsoft.AspNetCore.Identity;
using AccountingApi.DTOs.Authentication;
using AccountingApi.Models;
using AccountingApi.Services;
using System.Security.Claims;

namespace AccountingApi.Features.Authentication;

// Command to handle token refresh
using MyMediator;
public record RefreshTokenCommand(RefreshTokenRequestDto RefreshTokenRequest) : IRequest<ApiResponseDto<LoginResponseDto>>;

// Handler for RefreshToken
public class RefreshTokenHandler(
    UserManager<ApplicationUser> userManager,
    IJwtService jwtService,
    ILogger<RefreshTokenHandler> logger) : IRequestHandler<RefreshTokenCommand, ApiResponseDto<LoginResponseDto>>
{
    public async Task<ApiResponseDto<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = jwtService.GetPrincipalFromExpiredToken(request.RefreshTokenRequest.AccessToken);
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

        var user = await userManager.FindByIdAsync(userId);
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
        var newAccessToken = await jwtService.GenerateAccessTokenAsync(user);
        var newRefreshToken = jwtService.GenerateRefreshToken();
        
        // Save new refresh token
        await jwtService.SaveRefreshTokenAsync(user, newRefreshToken);

        var userRoles = await userManager.GetRolesAsync(user);

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

        logger.LogInformation("Token refreshed successfully for user {UserId}", userId);

        return new ApiResponseDto<LoginResponseDto>
        {
            Success = true,
            Message = "Token refreshed successfully.",
            Data = response
        };
    }
}
