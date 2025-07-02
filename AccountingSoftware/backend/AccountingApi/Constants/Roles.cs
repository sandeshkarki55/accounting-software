namespace AccountingApi.Constants;

/// <summary>
/// Contains constants for user roles used throughout the application.
/// </summary>
public static class Roles
{
    /// <summary>
    /// Administrator role with full system access.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Manager role with elevated privileges.
    /// </summary>
    public const string Manager = "Manager";

    /// <summary>
    /// Standard user role with basic access.
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Accountant role with accounting-specific permissions.
    /// </summary>
    public const string Accountant = "Accountant";

    /// <summary>
    /// Combined roles for management-level access (Admin + Manager).
    /// </summary>
    public static readonly string[] ManagementRoles = { Admin, Manager };

    /// <summary>
    /// Combined roles for financial access (Admin + Manager + Accountant).
    /// </summary>
    public static readonly string[] FinancialRoles = { Admin, Manager, Accountant };

    /// <summary>
    /// All available roles in the system.
    /// </summary>
    public static readonly string[] AllRoles = { Admin, Manager, User, Accountant };
}
