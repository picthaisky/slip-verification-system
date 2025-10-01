namespace SlipVerification.Domain.Enums;

/// <summary>
/// Represents the verification status of a slip
/// </summary>
public enum VerificationStatus
{
    /// <summary>
    /// Slip is pending verification
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Slip is being processed by OCR
    /// </summary>
    Processing = 1,
    
    /// <summary>
    /// Slip has been verified successfully
    /// </summary>
    Verified = 2,
    
    /// <summary>
    /// Slip verification failed
    /// </summary>
    Failed = 3,
    
    /// <summary>
    /// Slip has been rejected
    /// </summary>
    Rejected = 4,
    
    /// <summary>
    /// Slip requires manual review
    /// </summary>
    ManualReview = 5
}
