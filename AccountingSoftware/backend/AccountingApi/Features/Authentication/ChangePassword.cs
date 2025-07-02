using MediatR;
using Microsoft.AspNetCore.Identity;
using AccountingApi.DTOs.Authentication;
using AccountingApi.Models;

namespace AccountingApi.Features.Authentication;

// Command to handle password change
public record ChangePasswordCommand(string UserId, ChangePasswordRequestDto ChangePasswordRequest) : IRequest<ApiResponseDto<string>>;

// Handler for ChangePassword
public class ChangePasswordHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<ChangePasswordHandler> logger) : IRequestHandler<ChangePasswordCommand, ApiResponseDto<string>>
{
    public async Task<ApiResponseDto<string>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new ApiResponseDto<string>
            {
                Success = false,
                Message = "User not found.",
                Errors = ["User not found"]
            };
        }

        var result = await userManager.ChangePasswordAsync(user, request.ChangePasswordRequest.CurrentPassword, request.ChangePasswordRequest.NewPassword);
        
        if (!result.Succeeded)
        {
            return new ApiResponseDto<string>
            {
                Success = false,
                Message = "Failed to change password.",
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        logger.LogInformation("Password changed successfully for user {UserId}", request.UserId);

        return new ApiResponseDto<string>
        {
            Success = true,
            Message = "Password changed successfully.",
            Data = "Password updated"
        };
    }
}
