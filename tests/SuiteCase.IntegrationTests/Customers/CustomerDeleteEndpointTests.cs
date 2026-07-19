using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SuiteCase.Server.Data;

namespace SuiteCase.IntegrationTests.Customers;

[Collection(SqlServerCollection.Name)]
public sealed class CustomerDeleteEndpointTests(SqlServerFixture sqlServer)
    : CustomerEndpointTestBase(sqlServer)
{
    [Fact]
    public async Task SoftDeleteCustomer_PersistsDeletionAndReturnsNotFound_WhenDeletedAgain()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var created = await CreateCustomerAsync(client, CreateRequest());

        var deleteResponse = await client.DeleteAsync($"/api/customers/{created.Id}", TestCancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SuiteCaseDbContext>();
            var deletedCustomer = await db.Customers
                .IgnoreQueryFilters()
                .SingleAsync(customer => customer.Id == created.Id, TestCancellationToken);

            Assert.True(deletedCustomer.IsDeleted);
            Assert.NotNull(deletedCustomer.DeletedAt);
        }

        var secondDeleteResponse = await client.DeleteAsync(
            $"/api/customers/{created.Id}",
            TestCancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, secondDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task SoftDeleteCustomer_ReturnsProblemDetails404_WhenCustomerDoesNotExist()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.DeleteAsync("/api/customers/99999", TestCancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestCancellationToken);
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Customer not found", problem.Title);
        Assert.Equal("Customer not found.", problem.Detail);

        Assert.True(problem.Extensions.ContainsKey("code"));
        Assert.Equal("customer.not_found", problem.Extensions["code"]!.ToString());

        Assert.True(problem.Extensions.ContainsKey("traceId"));
        Assert.False(string.IsNullOrWhiteSpace(problem.Extensions["traceId"]!.ToString()));
    }
}
