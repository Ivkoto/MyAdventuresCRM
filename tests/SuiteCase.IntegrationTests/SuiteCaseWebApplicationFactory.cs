using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuiteCase.Core.Security;
using SuiteCase.Server.Data;

namespace SuiteCase.IntegrationTests;

internal sealed class SuiteCaseWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseConnectionString;

    public SuiteCaseWebApplicationFactory(string serverConnectionString)
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(serverConnectionString)
        {
            InitialCatalog = $"SuiteCaseIntegrationTests_{Guid.NewGuid():N}"            
        };

        _databaseConnectionString = connectionStringBuilder.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<SuiteCaseDbContext>>();
            services.RemoveAll<DbContextOptions<SuiteCaseDbContext>>();
            services.RemoveAll<SuiteCaseDbContext>();
            services.RemoveAll<ISensitiveDataProtector>();

            services.AddDbContext<SuiteCaseDbContext>(options =>
                options.UseSqlServer(_databaseConnectionString));

            services.AddScoped<ISensitiveDataProtector, FakeSensitiveDataProtector>();

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SuiteCaseDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
