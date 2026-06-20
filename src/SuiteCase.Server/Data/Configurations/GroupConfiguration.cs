using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");

        builder.HasKey(g => g.Id);

        // Alternate key used by composite FKs to enforce same-ProgramId integrity
        builder.HasAlternateKey(g => new { g.Id, g.ProgramId });

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
        builder.HasOne<TravelProgram>()
            .WithMany()
            .HasForeignKey(g => g.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-reference: child { ParentGroupId, ProgramId } -> parent { Id, ProgramId }
        // Enforces that a child group can only point to a parent with the same ProgramId.
        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(g => new { g.ParentGroupId, g.ProgramId })
            .HasPrincipalKey(g => new { g.Id, g.ProgramId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => g.ProgramId);
        builder.HasIndex(g => g.ParentGroupId);
        builder.HasIndex(g => new { g.ProgramId, g.Name });
    }
}
