namespace SlipVerification.Application.Interfaces;

/// <summary>
/// Interface for password hashing
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password
    /// </summary>
    string HashPassword(string password);
    
    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}

/// <summary>
/// Interface for password validation
/// </summary>
public interface IPasswordValidator
{
    /// <summary>
    /// Validates a password against policy rules
    /// </summary>
    PasswordValidationResult Validate(string password);
}

/// <summary>
/// Result of password validation
/// </summary>
public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
