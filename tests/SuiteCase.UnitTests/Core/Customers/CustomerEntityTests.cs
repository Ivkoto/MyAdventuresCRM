using SuiteCase.Core.Entities;

namespace SuiteCase.UnitTests.Core.Customers;

public sealed class CustomerEntityTests
{
    [Fact]
    public void SoftDeleted_SetSuccessfully()
    {
        var customer = CreateCustomer();
        var deletedAt = DateTimeOffset.UtcNow;
        
        customer.SoftDelete(deletedAt);

        Assert.Equal(deletedAt, customer.DeletedAt);
        Assert.True(customer.IsDeleted);
    }

    [Fact]
    public void SoftDelete_DoesNotOverwriteDeletedAt_WhenAlreadyDeleted()
    {
        var customer = CreateCustomer();
        var firstDeletedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var secondDeletedAt = DateTimeOffset.UtcNow;

        customer.SoftDelete(firstDeletedAt);
        customer.SoftDelete(secondDeletedAt);
        
        Assert.Equal(firstDeletedAt, customer.DeletedAt);
        Assert.NotEqual(secondDeletedAt, customer.DeletedAt);
    }

    private static Customer CreateCustomer()
        => new() { FirstName = "Ivan", LastName = "Petrov", CreatedAt = DateTimeOffset.UtcNow.AddDays(-5) };
}
