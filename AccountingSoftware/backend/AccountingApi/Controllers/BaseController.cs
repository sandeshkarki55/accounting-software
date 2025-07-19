using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingApi.Controllers;

/// <summary>
/// Base controller that enforces authentication for all derived controllers.
/// All controllers that inherit from this will require the user to be authenticated.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the current user's ID from the JWT token claims.
    /// </summary>
    protected string? UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// Gets the current user's username from the JWT token claims.
    /// </summary>
    protected string? Username => User.FindFirst(ClaimTypes.Name)?.Value;

    /// <summary>
    /// Gets the current user's email from the JWT token claims.
    /// </summary>
    protected string? UserEmail => User.FindFirst(ClaimTypes.Email)?.Value;
}