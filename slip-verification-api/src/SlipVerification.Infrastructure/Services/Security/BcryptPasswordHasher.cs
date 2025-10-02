using SlipVerification.Application.Interfaces;

namespace SlipVerification.Infrastructure.Services.Security;

/// <summary>
/// BCrypt-based password hasher implementation
/// </summary>
public class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // Cost factor for BCrypt

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
