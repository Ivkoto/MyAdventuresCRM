using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class TravelProgramOptionConfiguration : IEntityTypeConfiguration<TravelProgramOption>
{
    public void Configure(EntityTypeBuilder<TravelProgramOption> builder)
    {
        builder.ToTable("TravelProgramOptions");

        builder.HasKey(po => po.Id);

        builder.Property(po => po.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(po => po.Description)
            .HasMaxLength(4000);

        builder.Property(po => po.Notes)
            .HasMaxLength(4000);

        builder.Property(po => po.CreatedAt).IsRequired();
        builder.Property(po => po.UpdatedAt);
        builder.Property(po => po.DeletedAt);

        builder.HasQueryFilter(po => po.DeletedAt == null);

        builder.HasIndex(po => new { po.ProgramId, po.Name });

        builder.HasIndex(po => new { po.ProgramId, po.Name })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL")
            .HasDatabaseName("IX_ProgramOptions_ProgramId_Name_Unique");
    }
}
