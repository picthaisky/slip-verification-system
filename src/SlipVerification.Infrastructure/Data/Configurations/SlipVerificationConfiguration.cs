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

        builder.Property(s => s.Amount)
            .HasPrecision(18, 2);

        builder.Property(s => s.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(s => s.BankName)
            .HasMaxLength(100);

        builder.Property(s => s.SenderAccount)
            .HasMaxLength(200);

        builder.Property(s => s.ReceiverAccount)
            .HasMaxLength(200);

        builder.Property(s => s.OcrConfidence)
            .HasPrecision(5, 4);

        builder.Property(s => s.VerificationNotes)
            .HasMaxLength(1000);

        builder.HasIndex(s => s.OrderId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.CreatedAt);
        builder.HasIndex(s => s.IsDeleted);

        builder.HasMany(s => s.Transactions)
            .WithOne(t => t.SlipVerification)
            .HasForeignKey(t => t.SlipVerificationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
