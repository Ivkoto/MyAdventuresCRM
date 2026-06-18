using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class BookingTravelLegConfiguration : IEntityTypeConfiguration<BookingTravelLeg>
{
    public void Configure(EntityTypeBuilder<BookingTravelLeg> builder)
    {
        builder.ToTable("BookingTravelLegs");

        builder.HasKey(tl => tl.Id);

        builder.Property(tl => tl.Location)
            .HasMaxLength(150);

        builder.Property(tl => tl.Notes)
            .HasMaxLength(4000);

        builder.Property(tl => tl.CreatedAt).IsRequired();
        builder.Property(tl => tl.UpdatedAt);
        builder.Property(tl => tl.DeletedAt);

        builder.HasQueryFilter(tl => tl.DeletedAt == null);

        builder.HasOne<Booking>()
            .WithMany()
            .HasForeignKey(tl => tl.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(tl => new { tl.BookingId, tl.Direction })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
