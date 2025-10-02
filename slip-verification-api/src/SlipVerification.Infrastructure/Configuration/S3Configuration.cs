namespace SlipVerification.Infrastructure.Configuration;

/// <summary>
/// Configuration for AWS S3 storage
/// </summary>
public class S3Configuration
{
    /// <summary>
    /// AWS region
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// S3 bucket name
    /// </summary>
    public string BucketName { get; set; } = "slip-files";

    /// <summary>
    /// AWS access key ID
    /// </summary>
    public string? AccessKeyId { get; set; }

    /// <summary>
    /// AWS secret access key
    /// </summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>
    /// CloudFront distribution domain (optional)
    /// </summary>
    public string? CloudFrontDistributionDomain { get; set; }
}
