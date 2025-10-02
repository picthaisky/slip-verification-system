using SlipVerification.Domain.Entities;

namespace SlipVerification.Application.Interfaces;

/// <summary>
/// Interface for audit logging
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs an audit entry
    /// </summary>
    Task LogAsync(
        string action,
        string entityType,
        Guid? entityId = null,
        object? oldValues = null,
        object? newValues = null);
    
    /// <summary>
    /// Gets audit logs for an entity
    /// </summary>
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string entityType, Guid entityId);
    
    /// <summary>
    /// Gets audit logs for a user
    /// </summary>
    Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(Guid userId);
}
