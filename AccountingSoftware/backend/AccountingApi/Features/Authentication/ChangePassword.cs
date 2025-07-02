using MediatR;
using Microsoft.AspNetCore.Identity;
using AccountingApi.DTOs.Authentication;
using AccountingApi.Models;

namespace AccountingApi.Features.Authentication;

// Command to handle password change
public record ChangePasswordCommand(string UserId, ChangePasswordRequestDto ChangePasswordRequest) : IRequest<ApiResponseDto<string>>;

// Handler for ChangePassword
public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ApiResponseDto<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ChangePasswordHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ApiResponseDto<string>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new ApiResponseDto<string>
            {
                Success = false,
                Message = "User not found.",
                Errors = ["User not found"]
            };
        }

        var result = await _userManager.ChangePasswordAsync(user, request.ChangePasswordRequest.CurrentPassword, request.ChangePasswordRequest.NewPassword);
        
        if (!result.Succeeded)
        {
            return new ApiResponseDto<string>
            {
                Success = false,
                Message = "Failed to change password.",
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        _logger.LogInformation("Password changed successfully for user {UserId}", request.UserId);

        return new ApiResponseDto<string>
        {
            Success = true,
            Message = "Password changed successfully.",
            Data = "Password updated"
        };
    }
}
