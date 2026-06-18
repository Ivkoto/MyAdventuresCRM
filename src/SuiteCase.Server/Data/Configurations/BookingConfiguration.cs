using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.ProgramName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.GroupName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.Notes)
            .HasMaxLength(4000);

        builder.Property(b => b.BasePriceAmount)
            .HasPrecision(18, 2);

        builder.Property(b => b.TotalDiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(b => b.FinalPriceAmount)
            .HasPrecision(18, 2);

        builder.Property(b => b.AppliedLoyaltyRuleName)
            .HasMaxLength(150);

        builder.Property(b => b.AppliedLoyaltyDiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt);
        builder.Property(b => b.DeletedAt);

        builder.HasQueryFilter(b => b.DeletedAt == null);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Composite FK: Booking { GroupId, ProgramId } -> Group { Id, ProgramId }
        // Enforces that the selected Group belongs to the same ProgramId stored on Booking.
        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(b => new { b.GroupId, b.ProgramId })
            .HasPrincipalKey(b => new { b.Id, b.ProgramId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LoyaltyDiscountRule>()
            .WithMany()
            .HasForeignKey(b => b.AppliedLoyaltyRuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.CustomerId);
        builder.HasIndex(b => b.ProgramId);
        builder.HasIndex(b => b.GroupId);
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.BookedOn);
    }
}
