using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class BookingOptionConfiguration : IEntityTypeConfiguration<BookingOption>
{
    public void Configure(EntityTypeBuilder<BookingOption> builder)
    {
        builder.ToTable("BookingOptions");

        builder.HasKey(bo => bo.Id);

        builder.Property(bo => bo.OptionName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(bo => bo.PriceAmount)
            .HasPrecision(18, 2);

        builder.Property(bo => bo.CreatedAt).IsRequired();
        builder.Property(bo => bo.UpdatedAt);
        builder.Property(bo => bo.DeletedAt);

        builder.HasQueryFilter(bo => bo.DeletedAt == null);

        builder.HasOne<Booking>()
            .WithMany()
            .HasForeignKey(bo => bo.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TravelProgramOption>()
            .WithMany()
            .HasForeignKey(bo => bo.ProgramOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bo => new { bo.BookingId, bo.ProgramOptionId })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
