using Microsoft.EntityFrameworkCore;

namespace SuiteCase.Server.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<SuiteCaseDbContext>();

        await db.Database.MigrateAsync();
    }
}
