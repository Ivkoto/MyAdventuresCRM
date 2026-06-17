using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

using Program = SuiteCase.Core.Entities.Program;

public sealed class ProgramConfiguration : IEntityTypeConfiguration<Program>
{
    public void Configure(EntityTypeBuilder<Program> builder)
    {
        builder.ToTable("Programs");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.OrganizerName)
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(4000);

        builder.Property(p => p.Notes)
            .HasMaxLength(4000);

        builder.Property(p => p.BasePriceAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt);
        builder.Property(p => p.DeletedAt);

        builder.HasQueryFilter(p => p.DeletedAt == null);

        builder.HasMany<ProgramOption>()
            .WithOne()
            .HasForeignKey(po => po.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
