using Microsoft.AspNetCore.Identity;
using AccountingApi.DTOs.Authentication;
using AccountingApi.Models;

namespace AccountingApi.Features.Authentication;

// Command to handle user profile update
using MyMediator;
public record UpdateUserProfileCommand(string UserId, UpdateUserProfileDto UpdateProfileRequest) : IRequest<ApiResponseDto<UserInfoDto>>;

// Handler for UpdateUserProfileCommand
public class UpdateUserProfileHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<UpdateUserProfileHandler> logger) : IRequestHandler<UpdateUserProfileCommand, ApiResponseDto<UserInfoDto>>
{
    public async Task<ApiResponseDto<UserInfoDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            logger.LogWarning("User with ID {UserId} not found", request.UserId);
            return new ApiResponseDto<UserInfoDto>
            {
                Success = false,
                Message = "User not found.",
                Errors = ["User not found"]
            };
        }

        // Check if email is being changed and if it's already taken by another user
        if (!string.Equals(user.Email, request.UpdateProfileRequest.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await userManager.FindByEmailAsync(request.UpdateProfileRequest.Email);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return new ApiResponseDto<UserInfoDto>
                {
                    Success = false,
                    Message = "Email is already taken by another user.",
                    Errors = ["Email already exists"]
                };
            }
        }

        // Update user properties
        user.FirstName = request.UpdateProfileRequest.FirstName.Trim();
        user.LastName = request.UpdateProfileRequest.LastName.Trim();
        user.Email = request.UpdateProfileRequest.Email.Trim();
        user.UserName = request.UpdateProfileRequest.Email.Trim(); // Update username to match email
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to update user profile for user {UserId}. Errors: {Errors}", 
                request.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
            
            return new ApiResponseDto<UserInfoDto>
            {
                Success = false,
                Message = "Failed to update user profile.",
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        // Get updated user roles
        var roles = await userManager.GetRolesAsync(user);

        var userInfo = new UserInfoDto
        {
            Id = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Roles = roles.ToList()
        };

        logger.LogInformation("User profile updated successfully for user {UserId}", request.UserId);

        return new ApiResponseDto<UserInfoDto>
        {
            Success = true,
            Message = "Profile updated successfully.",
            Data = userInfo
        };
    }
}
