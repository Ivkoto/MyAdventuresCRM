using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class LoyaltyDiscountRuleConfiguration : IEntityTypeConfiguration<LoyaltyDiscountRule>
{
    public void Configure(EntityTypeBuilder<LoyaltyDiscountRule> builder)
    {
        builder.ToTable("LoyaltyDiscountRules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(r => r.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(r => r.ProgramPriceMinAmount)
            .HasPrecision(18, 2);

        builder.Property(r => r.ProgramPriceMaxAmount)
            .HasPrecision(18, 2);

        builder.Property(r => r.Notes)
            .HasMaxLength(4000);

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt);
        builder.Property(r => r.DeletedAt);

        builder.HasQueryFilter(r => r.DeletedAt == null);

        builder.HasIndex(r => new { r.EffectiveFrom, r.EffectiveTo });
        builder.HasIndex(r => new { r.TripCountFrom, r.TripCountTo });
        builder.HasIndex(r => r.Priority);
    }
}
