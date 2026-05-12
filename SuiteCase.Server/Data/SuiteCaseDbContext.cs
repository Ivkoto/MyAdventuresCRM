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
}
