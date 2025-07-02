using MediatR;
using Microsoft.AspNetCore.Identity;
using AccountingApi.DTOs.Authentication;
using AccountingApi.Models;

namespace AccountingApi.Features.Authentication;

// Query to get current user information
public record GetCurrentUserQuery(string UserId) : IRequest<ApiResponseDto<UserInfoDto>>;

// Handler for GetCurrentUser
public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, ApiResponseDto<UserInfoDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GetCurrentUserHandler> _logger;

    public GetCurrentUserHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<GetCurrentUserHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ApiResponseDto<UserInfoDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", request.UserId);
            return new ApiResponseDto<UserInfoDto>
            {
                Success = false,
                Message = "User not found.",
                Errors = ["User not found"]
            };
        }

        var roles = await _userManager.GetRolesAsync(user);

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
