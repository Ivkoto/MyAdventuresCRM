using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SuiteCase.Core.Entities;
using SuiteCase.Server.Data;
using SuiteCase.Server.Data.ErrorHandling;

namespace SuiteCase.IntegrationTests.ErrorHandling;

[Collection(SqlServerCollection.Name)]
public sealed class SqlServerExceptionClassifierTests(SqlServerFixture sqlServer)
{
    [Fact]
    public async Task IsUniqueConstraintViolation_ReturnsTrue_ForSqlServerUniqueIndexViolation()
    {
        using var factory = new SuiteCaseWebApplicationFactory(sqlServer.ConnectionString);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SuiteCaseDbContext>();

        var firstCustomer = CreateCustomer("Ivan", "Petrov");
        firstCustomer.SetNationalId("protected:first", "duplicate-hash");
        db.Customers.Add(firstCustomer);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var secondCustomer = CreateCustomer("Petar", "Ivanov");
        secondCustomer.SetNationalId("protected:second", "duplicate-hash");
        db.Customers.Add(secondCustomer);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => db.SaveChangesAsync(TestContext.Current.CancellationToken));

        Assert.True(SqlServerExceptionClassifier.IsUniqueConstraintViolation(exception));
    }

    [Fact]
    public void IsUniqueConstraintViolation_ReturnsFalse_ForUnrelatedUpdateException()
    {
        var exception = new DbUpdateException("Unrelated failure", new InvalidOperationException());

        Assert.False(SqlServerExceptionClassifier.IsUniqueConstraintViolation(exception));
    }

    private static Customer CreateCustomer(string firstName, string lastName)
        => new()
        {
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTimeOffset.UtcNow
        };
}
