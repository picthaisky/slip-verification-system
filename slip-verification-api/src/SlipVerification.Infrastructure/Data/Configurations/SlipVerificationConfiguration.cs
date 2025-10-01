using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SlipVerification.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for SlipVerification
/// </summary>
public class SlipVerificationConfiguration : IEntityTypeConfiguration<Domain.Entities.SlipVerification>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.SlipVerification> builder)
    {
        builder.ToTable("SlipVerifications");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ImagePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.ImageHash)
            .HasMaxLength(64);

        builder.Property(s => s.Amount)
            .HasPrecision(12, 2);

        builder.Property(s => s.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(s => s.BankName)
            .HasMaxLength(100);

        builder.Property(s => s.BankAccountNumber)
            .HasMaxLength(50);

        builder.Property(s => s.OcrConfidence)
            .HasPrecision(5, 2);

        builder.Property(s => s.VerificationNotes)
            .HasMaxLength(1000);

        builder.HasIndex(s => s.OrderId);
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.ReferenceNumber).HasFilter("deleted_at IS NULL");
        builder.HasIndex(s => s.Status).HasFilter("deleted_at IS NULL");
        builder.HasIndex(s => s.ImageHash).IsUnique().HasFilter("deleted_at IS NULL");
        builder.HasIndex(s => s.CreatedAt);
        builder.HasIndex(s => s.IsDeleted);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Verifier)
            .WithMany()
            .HasForeignKey(s => s.VerifiedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Transactions)
            .WithOne(t => t.SlipVerification)
            .HasForeignKey(t => t.SlipVerificationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
