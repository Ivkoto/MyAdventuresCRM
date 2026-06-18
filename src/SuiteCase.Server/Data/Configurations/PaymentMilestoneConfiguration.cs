using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class PaymentMilestoneConfiguration : IEntityTypeConfiguration<PaymentMilestone>
{
    public void Configure(EntityTypeBuilder<PaymentMilestone> builder)
    {
        builder.ToTable("PaymentMilestones");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pm => pm.Amount)
            .HasPrecision(18, 2);

        builder.Property(pm => pm.Notes)
            .HasMaxLength(4000);

        builder.Property(pm => pm.CreatedAt).IsRequired();
        builder.Property(pm => pm.UpdatedAt);
        builder.Property(pm => pm.DeletedAt);

        builder.HasQueryFilter(pm => pm.DeletedAt == null);

        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(pm => pm.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pm => new { pm.GroupId, pm.Sequence })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");

        builder.HasIndex(pm => pm.DueBy);
    }
}
