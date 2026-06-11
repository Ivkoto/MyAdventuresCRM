using Microsoft.EntityFrameworkCore;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data;

public sealed class SuiteCaseDbContext(DbContextOptions<SuiteCaseDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SuiteCaseDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        foreach (var customer in ChangeTracker.Entries<Customer>())
        {
            if (customer.State == EntityState.Added)
            {
                customer.Entity.CreatedAt = now;
            }
            if (customer.State == EntityState.Modified)
            {
                customer.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(ct);
    }
}
