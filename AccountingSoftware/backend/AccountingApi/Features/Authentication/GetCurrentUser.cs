using AccountingApi.DTOs.Authentication;
using AccountingApi.Models;

using Microsoft.AspNetCore.Identity;

namespace AccountingApi.Features.Authentication;

// Query to get current user information
using MyMediator;

public record GetCurrentUserQuery(string UserId) : IRequest<ApiResponseDto<UserInfoDto>>;

// Handler for GetCurrentUser
public class GetCurrentUserHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<GetCurrentUserHandler> logger) : IRequestHandler<GetCurrentUserQuery, ApiResponseDto<UserInfoDto>>
{
    public async Task<ApiResponseDto<UserInfoDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
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

        return new ApiResponseDto<UserInfoDto>
        {
            Success = true,
            Message = "User information retrieved successfully.",
            Data = userInfo
        };
    }
}