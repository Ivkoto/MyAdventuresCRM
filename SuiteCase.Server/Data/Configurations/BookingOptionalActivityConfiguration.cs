using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class BookingOptionalActivityConfiguration : IEntityTypeConfiguration<BookingOptionalActivity>
{
    public void Configure(EntityTypeBuilder<BookingOptionalActivity> builder)
    {
        builder.ToTable("BookingOptionalActivities");

        builder.HasKey(ba => ba.Id);

        builder.Property(ba => ba.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(ba => ba.PriceAmount)
            .HasPrecision(18, 2);

        builder.Property(ba => ba.Notes)
            .HasMaxLength(4000);

        builder.Property(ba => ba.CreatedAt).IsRequired();
        builder.Property(ba => ba.UpdatedAt);
        builder.Property(ba => ba.DeletedAt);

        builder.HasQueryFilter(ba => ba.DeletedAt == null);

        builder.HasOne<Booking>()
            .WithMany()
            .HasForeignKey(ba => ba.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<GroupOptionalActivity>()
            .WithMany()
            .HasForeignKey(ba => ba.GroupOptionalActivityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ba => new { ba.BookingId, ba.GroupOptionalActivityId })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
