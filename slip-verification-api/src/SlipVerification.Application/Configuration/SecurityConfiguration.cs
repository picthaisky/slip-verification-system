namespace SlipVerification.Application.Configuration;

/// <summary>
/// JWT configuration options
/// </summary>
public class JwtConfiguration
{
    public const string SectionName = "Jwt";
    
    /// <summary>
    /// Gets or sets the secret key for signing tokens
    /// </summary>
    public string Secret { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the issuer
    /// </summary>
    public string Issuer { get; set; } = "SlipVerificationAPI";
    
    /// <summary>
    /// Gets or sets the audience
    /// </summary>
    public string Audience { get; set; } = "SlipVerificationClient";
    
    /// <summary>
    /// Gets or sets the access token expiration in minutes
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;
    
    /// <summary>
    /// Gets or sets the refresh token expiration in days
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
    
    /// <summary>
    /// Gets or sets the clock skew in minutes
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;
}

/// <summary>
/// Password policy configuration options
/// </summary>
public class PasswordPolicyOptions
{
    public const string SectionName = "PasswordPolicy";
    
    /// <summary>
    /// Gets or sets the minimum password length
    /// </summary>
    public int MinimumLength { get; set; } = 8;
    
    /// <summary>
    /// Gets or sets whether an uppercase letter is required
    /// </summary>
    public bool RequireUppercase { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether a lowercase letter is required
    /// </summary>
    public bool RequireLowercase { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether a digit is required
    /// </summary>
    public bool RequireDigit { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether a special character is required
    /// </summary>
    public bool RequireSpecialCharacter { get; set; } = true;
}

/// <summary>
/// Rate limiting configuration options
/// </summary>
public class RateLimitOptions
{
    public const string SectionName = "RateLimit";
    
    /// <summary>
    /// Gets or sets the maximum number of requests
    /// </summary>
    public int MaxRequests { get; set; } = 100;
    
    /// <summary>
    /// Gets or sets the time window in seconds
    /// </summary>
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(1);
}
