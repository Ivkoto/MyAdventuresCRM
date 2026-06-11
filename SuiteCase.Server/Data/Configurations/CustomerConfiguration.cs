using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.MiddleName)
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.FirstNameLatin)
            .HasMaxLength(100);

        builder.Property(c => c.MiddleNameLatin)
            .HasMaxLength(100);

        builder.Property(c => c.LastNameLatin)
            .HasMaxLength(100);

        builder.Property(c => c.Email)
            .HasMaxLength(254);

        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(c => c.ResidenceCountry)
            .HasMaxLength(50);

        builder.Property(c => c.NationalIdEncrypted)
            .HasMaxLength(512);

        builder.Property(c => c.NationalIdHash)
            .HasMaxLength(128);

        builder.Property(c => c.PassportNumberEncrypted)
            .HasMaxLength(512);

        builder.Property(c => c.PassportNumberHash)
            .HasMaxLength(128);

        builder.Property(c => c.Notes)
            .HasColumnType("nvarchar(max)");

         builder.Property(c => c.CreatedAt).IsRequired();
         builder.Property(c => c.UpdatedAt);
         builder.Property(c => c.DeletedAt);

        builder.HasQueryFilter(c => c.DeletedAt == null);

        //TODO: Check with the team what are the most used properties for searching a customer and correct indeces if needed.
        builder.HasIndex(c => new {c.FirstName, c.LastName});
        builder.HasIndex(c => c.PhoneNumber);
        builder.HasIndex(c => c.Email);

        builder.HasIndex(x => x.NationalIdHash).IsUnique().HasFilter("[NationalIdHash] IS NOT NULL AND [DeletedAt] IS NULL");
        builder.HasIndex(x => x.PassportNumberHash).IsUnique().HasFilter("[PassportNumberHash] IS NOT NULL AND [DeletedAt] IS NULL");
    }
}
