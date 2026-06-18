using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

using Program = SuiteCase.Core.Entities.Program;

public sealed class ProgramPricingRuleConfiguration : IEntityTypeConfiguration<ProgramPricingRule>
{
    public void Configure(EntityTypeBuilder<ProgramPricingRule> builder)
    {
        builder.ToTable("ProgramPricingRules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(r => r.AppliesTo)
            .HasMaxLength(50);

        builder.Property(r => r.Notes)
            .HasMaxLength(4000);

        builder.Property(r => r.PriceAmount)
            .HasPrecision(18, 2);

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt);
        builder.Property(r => r.DeletedAt);

        builder.HasQueryFilter(r => r.DeletedAt == null);

        builder.HasOne<Program>()
            .WithMany()
            .HasForeignKey(r => r.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(r => r.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.ProgramId);
        builder.HasIndex(r => r.GroupId);
        builder.HasIndex(r => new { r.ProgramId, r.GroupId, r.Name });
    }
}
