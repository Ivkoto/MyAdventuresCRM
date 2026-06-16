using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

using Program = SuiteCase.Core.Entities.Program;

public sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(g => g.DepartureLocation)
            .HasMaxLength(150);

        builder.Property(g => g.ReturnLocation)
            .HasMaxLength(150);

        builder.Property(g => g.CustomerContactName)
            .HasMaxLength(100);

        builder.Property(g => g.GuideName)
            .HasMaxLength(100);

        builder.Property(g => g.Description)
            .HasMaxLength(4000);

        builder.Property(g => g.Notes)
            .HasMaxLength(4000);

        builder.Property(g => g.CreatedAt).IsRequired();
        builder.Property(g => g.UpdatedAt);
        builder.Property(g => g.DeletedAt);

        builder.HasQueryFilter(g => g.DeletedAt == null);

        // FK to Program
        builder.HasOne<Program>()
            .WithMany()
            .HasForeignKey(g => g.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-reference: parent/children
        builder.HasMany<Group>()
            .WithOne()
            .HasForeignKey(g => g.ParentGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => g.ProgramId);
        builder.HasIndex(g => g.ParentGroupId);
        builder.HasIndex(g => new { g.ProgramId, g.Name });
    }
}
