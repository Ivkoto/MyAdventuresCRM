using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class LoyaltyDiscountRuleDestinationConfiguration : IEntityTypeConfiguration<LoyaltyDiscountRuleDestination>
{
    public void Configure(EntityTypeBuilder<LoyaltyDiscountRuleDestination> builder)
    {
        builder.ToTable("LoyaltyDiscountRuleDestinations");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.LoyaltyScopeKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.UpdatedAt);
        builder.Property(d => d.DeletedAt);

        builder.HasQueryFilter(d => d.DeletedAt == null);

        builder.HasOne<LoyaltyDiscountRule>()
            .WithMany()
            .HasForeignKey(d => d.RuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.RuleId, d.LoyaltyScopeKey })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
