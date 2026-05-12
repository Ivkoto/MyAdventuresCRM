using SuiteCase.Core.Entities;

namespace SuiteCase.UnitTests.Core.Entities;

public sealed class CustomerTests
{
    [Fact]
    public void SoftDeleted_SetSuccessfully()
    {
        var customer = CreateCustomer();
        var deletedAt = DateTime.UtcNow;
        
        customer.SoftDelete(deletedAt);

        Assert.Equal(deletedAt, customer.DeletedAt);
        Assert.True(customer.IsDeleted);
    }

    [Fact]
    public void SoftDelete_DoesNotOverwriteDeletedAt_WhenAlreadyDeleted()
    {
        var customer = CreateCustomer();
        var firstDeletedAt = DateTime.Now.AddDays(-1);
        var secondDeletedAt = DateTime.Now;

        customer.SoftDelete(firstDeletedAt);
        customer.SoftDelete(secondDeletedAt);
        
        Assert.Equal(firstDeletedAt, customer.DeletedAt);
        Assert.NotEqual(secondDeletedAt, customer.DeletedAt);
    }

    private static Customer CreateCustomer()
        => new() { FirstName = "Ivan", LastName = "Petrov", CreatedAt = DateTime.UtcNow.AddDays(-5)};
}