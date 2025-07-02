using MediatR;
using Microsoft.AspNetCore.Identity;
using AccountingApi.DTOs.Authentication;
using AccountingApi.Models;
using AccountingApi.Services;

namespace AccountingApi.Features.Authentication;

// Command to handle user login
public record LoginCommand(LoginRequestDto LoginRequest) : IRequest<ApiResponseDto<LoginResponseDto>>;

// Handler for LoginCommand
public class LoginHandler : IRequestHandler<LoginCommand, ApiResponseDto<LoginResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        ILogger<LoginHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<ApiResponseDto<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.LoginRequest.Email);
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

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.LoginRequest.Password, lockoutOnFailure: true);
        
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
        var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        // Save refresh token
        await _jwtService.SaveRefreshTokenAsync(user, refreshToken);

        var userRoles = await _userManager.GetRolesAsync(user);

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

        _logger.LogInformation("User {Email} logged in successfully", request.LoginRequest.Email);

        return new ApiResponseDto<LoginResponseDto>
        {
            Success = true,
            Message = "Login successful.",
            Data = response
        };
    }
}
