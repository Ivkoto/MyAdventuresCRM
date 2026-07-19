using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuiteCase.Core.Security;
using SuiteCase.Server.Data;

namespace SuiteCase.IntegrationTests;

internal sealed class SuiteCaseWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseConnectionString;
    private readonly IInterceptor? _databaseInterceptor;

    public SuiteCaseWebApplicationFactory(
        string serverConnectionString,
        IInterceptor? databaseInterceptor = null)
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(serverConnectionString)
        {
            InitialCatalog = $"SuiteCaseIntegrationTests_{Guid.NewGuid():N}"
        };

        _databaseConnectionString = connectionStringBuilder.ConnectionString;
        _databaseInterceptor = databaseInterceptor;
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
            {
                options.UseSqlServer(
                    _databaseConnectionString,
                    sqlOptions => sqlOptions.EnableRetryOnFailure());

                if (_databaseInterceptor is not null)
                    options.AddInterceptors(_databaseInterceptor);
            });

            services.AddScoped<ISensitiveDataProtector, FakeSensitiveDataProtector>();

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SuiteCaseDbContext>();
            db.Database.Migrate();
        });
    }
}
