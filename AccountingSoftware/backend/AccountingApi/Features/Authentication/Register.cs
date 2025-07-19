using Microsoft.AspNetCore.Identity;
using AccountingApi.DTOs.Authentication;
using AccountingApi.Models;

namespace AccountingApi.Features.Authentication;

// Command to handle user registration
using MyMediator;
public record RegisterCommand(RegisterRequestDto RegisterRequest) : IRequest<ApiResponseDto<UserInfoDto>>;

// Handler for RegisterCommand
public class RegisterHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<RegisterHandler> logger) : IRequestHandler<RegisterCommand, ApiResponseDto<UserInfoDto>>
{
    public async Task<ApiResponseDto<UserInfoDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(request.RegisterRequest.Email);
        if (existingUser != null)
        {
            return new ApiResponseDto<UserInfoDto>
            {
                Success = false,
                Message = "User with this email already exists.",
                Errors = ["Email already registered"]
            };
        }

        // Create new user
        var user = new ApplicationUser
        {
            UserName = request.RegisterRequest.Email,
            Email = request.RegisterRequest.Email,
            FirstName = request.RegisterRequest.FirstName,
            LastName = request.RegisterRequest.LastName,
            EmailConfirmed = true, // For simplicity, auto-confirm emails
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, request.RegisterRequest.Password);
        
        if (!result.Succeeded)
        {
            return new ApiResponseDto<UserInfoDto>
            {
                Success = false,
                Message = "Failed to create user.",
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        // Assign default role
        await userManager.AddToRoleAsync(user, "User");

        var userInfo = new UserInfoDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Roles = ["User"]
        };

        logger.LogInformation("User {Email} registered successfully", request.RegisterRequest.Email);

        return new ApiResponseDto<UserInfoDto>
        {
            Success = true,
            Message = "Registration successful.",
            Data = userInfo
        };
    }
}
