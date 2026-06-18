using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class GroupOptionConfiguration : IEntityTypeConfiguration<GroupOption>
{
    public void Configure(EntityTypeBuilder<GroupOption> builder)
    {
        builder.ToTable("GroupOptions");

        builder.HasKey(go => go.Id);

        builder.Property(go => go.PriceAmount)
            .HasPrecision(18, 2);

        builder.Property(go => go.Notes)
            .HasMaxLength(4000);

        builder.Property(go => go.CreatedAt).IsRequired();
        builder.Property(go => go.UpdatedAt);
        builder.Property(go => go.DeletedAt);

        builder.HasQueryFilter(go => go.DeletedAt == null);

        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(go => go.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProgramOption>()
            .WithMany()
            .HasForeignKey(go => go.ProgramOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(go => new { go.GroupId, go.ProgramOptionId })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
