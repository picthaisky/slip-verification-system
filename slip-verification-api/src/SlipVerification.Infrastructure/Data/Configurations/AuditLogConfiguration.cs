using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlipVerification.Domain.Entities;

namespace SlipVerification.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for AuditLog
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.OldValues)
            .HasColumnType("jsonb");

        builder.Property(a => a.NewValues)
            .HasColumnType("jsonb");

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45);

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.CreatedAt).IsDescending();

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
