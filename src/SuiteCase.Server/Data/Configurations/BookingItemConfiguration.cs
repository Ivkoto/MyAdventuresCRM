using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
{
    public void Configure(EntityTypeBuilder<BookingItem> builder)
    {
        builder.ToTable("BookingItems");

        builder.HasKey(bi => bi.Id);

        builder.Property(bi => bi.Type)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(bi => bi.Description)
            .HasMaxLength(4000);

        builder.Property(bi => bi.Amount)
            .HasPrecision(18, 2);

        builder.Property(bi => bi.CreatedAt).IsRequired();
        builder.Property(bi => bi.UpdatedAt);
        builder.Property(bi => bi.DeletedAt);

        builder.HasQueryFilter(bi => bi.DeletedAt == null);

        builder.HasOne<Booking>()
            .WithMany()
            .HasForeignKey(bi => bi.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TravelProgramPricingRule>()
            .WithMany()
            .HasForeignKey(bi => bi.ProgramPricingRuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bi => bi.BookingId);
        builder.HasIndex(bi => bi.ProgramPricingRuleId);
    }
}
