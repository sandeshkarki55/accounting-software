using System.Security.Claims;

namespace AccountingApi.Services.CurrentUserService;

/// <summary>
/// Implementation of ICurrentUserService that provides current user context from HTTP context
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current user's ID from the JWT token claims
    /// </summary>
    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// Gets the current user's username from the JWT token claims
    /// </summary>
    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    /// <summary>
    /// Gets the current user's email from the JWT token claims
    /// </summary>
    public string? UserEmail => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    /// <summary>
    /// Gets the current user identifier for audit purposes
    /// Returns the user's email if available, then username, then user ID, otherwise "System"
    /// </summary>
    public string GetCurrentUserForAudit()
    {
        // Return email if available (most user-friendly)
        if (!string.IsNullOrEmpty(UserEmail))
            return UserEmail;

        // Fall back to username
        if (!string.IsNullOrEmpty(Username))
            return Username;

        // Fall back to user ID
        if (!string.IsNullOrEmpty(UserId))
            return UserId;

        // Default fallback for non-authenticated contexts
        return "System";
    }
}