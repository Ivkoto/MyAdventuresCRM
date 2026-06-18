using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2);

        builder.Property(p => p.ExternalReference)
            .HasMaxLength(150);

        builder.Property(p => p.Notes)
            .HasMaxLength(4000);

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.ChangedBy)
            .HasMaxLength(100);

        builder.Property(p => p.Reason)
            .HasMaxLength(4000);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt);
        builder.Property(p => p.DeletedAt);

        builder.HasQueryFilter(p => p.DeletedAt == null);

        builder.HasOne<Booking>()
            .WithMany()
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.BookingId);
        builder.HasIndex(p => p.PaidOn);
        builder.HasIndex(p => p.Direction);
    }
}
