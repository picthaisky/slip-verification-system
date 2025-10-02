using Microsoft.Extensions.Options;
using SlipVerification.Application.Configuration;
using SlipVerification.Application.Interfaces;

namespace SlipVerification.Infrastructure.Services.Security;

/// <summary>
/// Password validator implementation
/// </summary>
public class PasswordValidator : IPasswordValidator
{
    private readonly PasswordPolicyOptions _options;

    public PasswordValidator(IOptions<PasswordPolicyOptions> options)
    {
        _options = options.Value;
    }

    public PasswordValidationResult Validate(string password)
    {
        var result = new PasswordValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(password))
        {
            result.IsValid = false;
            result.Errors.Add("Password is required");
            return result;
        }

        if (password.Length < _options.MinimumLength)
        {
            result.IsValid = false;
            result.Errors.Add($"Password must be at least {_options.MinimumLength} characters");
        }

        if (_options.RequireUppercase && !password.Any(char.IsUpper))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one uppercase letter");
        }

        if (_options.RequireLowercase && !password.Any(char.IsLower))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one lowercase letter");
        }

        if (_options.RequireDigit && !password.Any(char.IsDigit))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one digit");
        }

        if (_options.RequireSpecialCharacter &&
            !password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one special character");
        }

        return result;
    }
}
