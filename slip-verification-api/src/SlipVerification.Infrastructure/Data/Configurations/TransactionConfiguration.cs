using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlipVerification.Domain.Entities;

namespace SlipVerification.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Transaction
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(12, 2);

        builder.Property(t => t.TransactionType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(t => t.Description);

        builder.Property(t => t.Metadata)
            .HasColumnType("jsonb");

        builder.HasIndex(t => t.OrderId);
        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.CreatedAt).IsDescending();
        builder.HasIndex(t => t.IsDeleted);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Order)
            .WithMany(o => o.Transactions)
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.SlipVerification)
            .WithMany(s => s.Transactions)
            .HasForeignKey(t => t.SlipVerificationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
