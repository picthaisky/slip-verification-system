using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlipVerification.Domain.Entities;

namespace SlipVerification.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Notification
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(n => n.Message)
            .IsRequired();

        builder.Property(n => n.Data)
            .HasColumnType("jsonb");

        builder.Property(n => n.Channel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(n => n.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(n => n.RetryCount)
            .HasDefaultValue(0);

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.CreatedAt).IsDescending();
        builder.HasIndex(n => n.ReadAt).HasFilter("read_at IS NULL");
        builder.HasIndex(n => n.IsDeleted);

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
