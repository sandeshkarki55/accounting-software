namespace AccountingApi.Services.CurrentUserService;

/// <summary>
/// Service to provide current user context information for audit purposes
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID from the HTTP context
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the current user's username from the HTTP context
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Gets the current user's email from the HTTP context
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// Gets the current user's full name for audit purposes
    /// Returns a combination of username/email if available, otherwise "System"
    /// </summary>
    string GetCurrentUserForAudit();
}