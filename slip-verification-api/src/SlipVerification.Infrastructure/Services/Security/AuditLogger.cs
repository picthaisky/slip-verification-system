using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SlipVerification.Application.Interfaces;
using SlipVerification.Domain.Entities;
using SlipVerification.Infrastructure.Data;

namespace SlipVerification.Infrastructure.Services.Security;

/// <summary>
/// Audit logger implementation
/// </summary>
public class AuditLogger : IAuditLogger
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogger> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(
        string action,
        string entityType,
        Guid? entityId = null,
        object? oldValues = null,
        object? newValues = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var auditLog = new AuditLog
        {
            UserId = userId != null ? Guid.Parse(userId) : null,
            Action = action,
            EntityType = entityType,
            EntityId = entityId ?? Guid.Empty,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers["User-Agent"].ToString(),
            CreatedAt = DateTime.UtcNow
        };

        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Audit log created: {Action} on {EntityType} {EntityId} by user {UserId}",
            action, entityType, entityId, userId);
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string entityType, Guid entityId)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(Guid userId)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}
