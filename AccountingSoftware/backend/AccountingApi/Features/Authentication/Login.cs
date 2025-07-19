using Microsoft.AspNetCore.Identity;
using AccountingApi.DTOs.Authentication;
using AccountingApi.Models;
using AccountingApi.Services;

namespace AccountingApi.Features.Authentication;

// Command to handle user login
using MyMediator;
public record LoginCommand(LoginRequestDto LoginRequest) : IRequest<ApiResponseDto<LoginResponseDto>>;

// Handler for LoginCommand
public class LoginHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtService jwtService,
    ILogger<LoginHandler> logger) : IRequestHandler<LoginCommand, ApiResponseDto<LoginResponseDto>>
{
    public async Task<ApiResponseDto<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.LoginRequest.Email);
        if (user == null)
        {
            return new ApiResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "Invalid email or password.",
                Errors = ["Invalid credentials"]
            };
        }

        if (!user.IsActive)
        {
            return new ApiResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "Account is deactivated. Please contact administrator.",
                Errors = ["Account deactivated"]
            };
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.LoginRequest.Password, lockoutOnFailure: true);
        
        if (!result.Succeeded)
        {
            var errors = new List<string>();
            
            if (result.IsLockedOut)
                errors.Add("Account is locked out. Please try again later.");
            else if (result.IsNotAllowed)
                errors.Add("Account is not allowed to sign in. Please verify your email.");
            else
                errors.Add("Invalid email or password.");

            return new ApiResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "Login failed.",
                Errors = errors
            };
        }

        // Generate tokens
        var accessToken = await jwtService.GenerateAccessTokenAsync(user);
        var refreshToken = jwtService.GenerateRefreshToken();
        
        // Save refresh token
        await jwtService.SaveRefreshTokenAsync(user, refreshToken);

        var userRoles = await userManager.GetRolesAsync(user);

        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600, // 1 hour in seconds
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

        logger.LogInformation("User {Email} logged in successfully", request.LoginRequest.Email);

        return new ApiResponseDto<LoginResponseDto>
        {
            Success = true,
            Message = "Login successful.",
            Data = response
        };
    }
}
