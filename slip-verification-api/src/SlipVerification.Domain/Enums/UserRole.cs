namespace SlipVerification.Domain.Enums;

/// <summary>
/// Represents user roles in the system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Guest user with limited access
    /// </summary>
    Guest = 0,
    
    /// <summary>
    /// Regular user
    /// </summary>
    User = 1,
    
    /// <summary>
    /// Manager with elevated access
    /// </summary>
    Manager = 2,
    
    /// <summary>
    /// Administrator with full access
    /// </summary>
    Admin = 3
}
