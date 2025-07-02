using MediatR;
using Microsoft.AspNetCore.Identity;
using AccountingApi.DTOs.Authentication;
using AccountingApi.Models;

namespace AccountingApi.Features.Authentication;

// Command to handle user logout
public record LogoutCommand(string UserId) : IRequest<ApiResponseDto<string>>;

// Handler for Logout
public class LogoutHandler : IRequestHandler<LogoutCommand, ApiResponseDto<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<LogoutHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ApiResponseDto<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user != null)
        {
            // Clear refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        _logger.LogInformation("User {UserId} logged out successfully", request.UserId);

        return new ApiResponseDto<string>
        {
            Success = true,
            Message = "Logout successful.",
            Data = "Logged out"
        };
    }
}
