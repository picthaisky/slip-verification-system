namespace SlipVerification.Domain.Common;

/// <summary>
/// Base entity class with common properties
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who created this entity
    /// </summary>
    public Guid? CreatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the last update date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who last updated this entity
    /// </summary>
    public Guid? UpdatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether this entity is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Gets or sets the date when this entity was deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
