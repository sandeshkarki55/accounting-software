using AccountingApi.DTOs.Authentication;
using AccountingApi.Models;

using Microsoft.AspNetCore.Identity;

namespace AccountingApi.Features.Authentication;

// Command to handle user logout
using MyMediator;

public record LogoutCommand(string UserId) : IRequest<ApiResponseDto<string>>;

// Handler for Logout
public class LogoutHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<LogoutHandler> logger) : IRequestHandler<LogoutCommand, ApiResponseDto<string>>
{
    public async Task<ApiResponseDto<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user != null)
        {
            // Clear refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
        }

        logger.LogInformation("User {UserId} logged out successfully", request.UserId);

        return new ApiResponseDto<string>
        {
            Success = true,
            Message = "Logout successful.",
            Data = "Logged out"
        };
    }
}