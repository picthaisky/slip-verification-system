using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlipVerification.Domain.Entities;

namespace SlipVerification.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Order
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.Amount)
            .HasPrecision(18, 2);

        builder.Property(o => o.Description)
            .HasMaxLength(500);

        builder.Property(o => o.QrCodeData);

        builder.Property(o => o.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
        builder.HasIndex(o => o.IsDeleted);

        builder.HasMany(o => o.SlipVerifications)
            .WithOne(s => s.Order)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Transactions)
            .WithOne(t => t.Order)
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
