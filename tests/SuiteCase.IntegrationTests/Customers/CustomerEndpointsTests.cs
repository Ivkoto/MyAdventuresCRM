using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SuiteCase.Server.Data;
using SuiteCase.Server.Features.Customers;

namespace SuiteCase.IntegrationTests.Customers;

[Collection(SqlServerCollection.Name)]
public sealed class CustomerEndpointsTests(SqlServerFixture sqlServer)
{
    private readonly SqlServerFixture _sqlServer = sqlServer;

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static CreateCustomerRequest CreateRequest(
        string firstName = "Ivan",  string lastName = "Petrov",
        string nationalId = "ivan123", string passportNumber = "pa123456")
        => new (
                firstName,
                null,
                lastName,
                "Ivan",
                null,
                "Petrov",
                nationalId,
                new DateOnly(1990, 1, 15),
                passportNumber,
                new DateOnly(2030, 5, 1),
                "ivan.petrov@example.com",
                "+359888111222",
                "Bulgaria",
                "Test customer"
        );

    [Fact]
    public async Task CustomerCrudFlow_WorksCorrectly_WhenWeCRUDaCustomer()
    {
        using var factory = new SuiteCaseWebApplicationFactory(_sqlServer.ConnectionString);
        using var client = CreateClient(factory);

        var response = await client.PostAsJsonAsync("/api/customers", CreateRequest());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CustomerDetailsResponse>();
        Assert.NotNull(created);
        Assert.Equal("IVAN123", created.NationalId);
        Assert.Equal("PA123456", created.PassportNumber);

        var list = await client.GetFromJsonAsync<List<CustomerListResponse>>("/api/customers");
        Assert.NotNull(list);
        var listCustomer = Assert.Single(list);
        Assert.Equal(created.Id, listCustomer.Id);
        Assert.Equal("Ivan", listCustomer.FirstName);
        Assert.Equal("Petrov", listCustomer.LastName);

        var details = await client.GetFromJsonAsync<CustomerDetailsResponse>($"/api/customers/{created.Id}");
        Assert.NotNull(details);
        Assert.Equal(created.Id, details.Id);
        Assert.Equal("IVAN123", details.NationalId);

        var updateResponse = await client.PutAsJsonAsync($"/api/customers/{created.Id}",
            new UpdateCustomerRequest(
                "Ivan",
                "Nikolov",
                "Petrov",
                "Ivan",
                "Nikolov",
                "Petrov",
                "ivan123",
                new DateOnly(1990, 1, 15),
                "pa123456",
                new DateOnly(2030, 5, 1),
                "ivan.petrov@example.com",
                "+359888111222",
                "Bulgaria",
                "Updated notes"
            )
        );
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<CustomerDetailsResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Nikolov", updated.MiddleName);
        Assert.Equal("Updated notes", updated.Notes);
        Assert.Equal("IVAN123", updated.NationalId);

        var deleteResponse = await client.DeleteAsync($"/api/customers/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getDeletedResponse = await client.GetAsync($"/api/customers/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);

        var listAfterDelete = await client.GetFromJsonAsync<List<CustomerListResponse>>("/api/customers");
        Assert.NotNull(listAfterDelete);
        Assert.Empty(listAfterDelete);
    }

    [Fact]
    public async Task CreateCustomer_ReturnsConflict_WhenNationalIdBelongsToActiveCustomer()
    {
        using var factory = new SuiteCaseWebApplicationFactory(_sqlServer.ConnectionString);
        using var client = CreateClient(factory);

        var firstResponse = await client.PostAsJsonAsync("/api/customers",
            CreateRequest(nationalId: "ivan123", passportNumber: "pa123456"));

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var duplicateResponse = await client.PostAsJsonAsync("/api/customers",
            CreateRequest(
                firstName: "Petar",
                lastName: "Ivanov",
                nationalId: " IVAN123 ",
                passportNumber: "pb654321"));

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_ReturnsConflict_WhenPassportNumberBelongsToActiveCustomer()
    {
        using var factory = new SuiteCaseWebApplicationFactory(_sqlServer.ConnectionString);
        using var client = CreateClient(factory);

        var firstResponse = await client.PostAsJsonAsync("/api/customers",
            CreateRequest(nationalId: "ivan123", passportNumber: "pa123456"));

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var duplicateResponse = await client.PostAsJsonAsync("/api/customers",
            CreateRequest(
                firstName: "Petar",
                lastName: "Ivanov",
                nationalId: "petar456",
                passportNumber: " PA123456 "));

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_ReturnsBadRequest_WhenRequiredFieldsAreMissing()
    {
        using var factory = new SuiteCaseWebApplicationFactory(_sqlServer.ConnectionString);
        using var client = CreateClient(factory);

        var response = await client.PostAsJsonAsync("/api/customers",
            new
            {
                firstName = "",
                lastName = ""
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_EnsureStoresSensitiveValuesProtectedAndHashed_WhenAddToExistingOrNewCustomer()
    {
        using var factory = new SuiteCaseWebApplicationFactory(_sqlServer.ConnectionString);
        using var client = CreateClient(factory);

        var response = await client.PostAsJsonAsync("/api/customers",
            CreateRequest(nationalId: "ivan123", passportNumber: "pa123456"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CustomerDetailsResponse>();
        Assert.NotNull(created);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SuiteCaseDbContext>();
        var customer = await db.Customers
            .IgnoreQueryFilters()
            .SingleAsync(c => c.Id == created.Id);

        Assert.NotNull(customer.NationalIdEncrypted);
        Assert.NotNull(customer.NationalIdHash);
        Assert.NotNull(customer.PassportNumberEncrypted);
        Assert.NotNull(customer.PassportNumberHash);
        Assert.NotEqual("IVAN123", customer.NationalIdEncrypted);
        Assert.NotEqual("IVAN123", customer.NationalIdHash);
        Assert.NotEqual("PA123456", customer.PassportNumberEncrypted);
        Assert.NotEqual("PA123456", customer.PassportNumberHash);
    }

    [Fact]
    public async Task CreateCustomer_AllowsSameSensitiveValues_AfterSoftDelete()
    {
        using var factory = new SuiteCaseWebApplicationFactory(_sqlServer.ConnectionString);
        using var client = CreateClient(factory);

        var firstResponse = await client.PostAsJsonAsync(
            "/api/customers",
            CreateRequest(nationalId: "ivan123", passportNumber: "pa123456"));

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var firstCustomer = await firstResponse.Content.ReadFromJsonAsync<CustomerDetailsResponse>();
        Assert.NotNull(firstCustomer);

        var deleteResponse = await client.DeleteAsync($"/api/customers/{firstCustomer.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var recreateResponse = await client.PostAsJsonAsync(
            "/api/customers",
            CreateRequest(
                firstName: "Ivan",
                lastName: "Petrov",
                nationalId: "ivan123",
                passportNumber: "pa123456"));

        Assert.Equal(HttpStatusCode.Created, recreateResponse.StatusCode);
    }
}
