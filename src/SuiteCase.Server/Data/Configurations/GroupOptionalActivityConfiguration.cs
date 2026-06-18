using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class GroupOptionalActivityConfiguration : IEntityTypeConfiguration<GroupOptionalActivity>
{
    public void Configure(EntityTypeBuilder<GroupOptionalActivity> builder)
    {
        builder.ToTable("GroupOptionalActivities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasMaxLength(4000);

        builder.Property(a => a.Notes)
            .HasMaxLength(4000);

        builder.Property(a => a.PriceAmount)
            .HasPrecision(18, 2);

        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);
        builder.Property(a => a.DeletedAt);

        builder.HasQueryFilter(a => a.DeletedAt == null);

        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(a => a.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.GroupId, a.SortOrder });

        builder.HasIndex(a => new { a.GroupId, a.Name })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
