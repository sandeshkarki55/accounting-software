using System.Security.Claims;

using AccountingApi.Models;

namespace AccountingApi.Services.JwtService;

public interface IJwtService
{
    Task<string> GenerateAccessTokenAsync(ApplicationUser user);

    string GenerateRefreshToken();

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    Task<bool> SaveRefreshTokenAsync(ApplicationUser user, string refreshToken);
}
