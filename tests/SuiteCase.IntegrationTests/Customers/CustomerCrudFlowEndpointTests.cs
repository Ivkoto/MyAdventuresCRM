using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SuiteCase.Server.Features.Customers.DTO;

namespace SuiteCase.IntegrationTests.Customers;

[Collection(SqlServerCollection.Name)]
public sealed class CustomerCrudFlowEndpointTests(SqlServerFixture sqlServer)
    : CustomerEndpointTestBase(sqlServer)
{
    [Fact]
    public async Task CustomerCrudFlow_WorksCorrectly_WhenWeCRUDaCustomer()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/customers",
            CreateRequest(),
            TestCancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CustomerDetailsResponse>(TestCancellationToken);
        Assert.NotNull(created);
        Assert.Equal("Ivan", created.FirstName);
        Assert.Equal("Georgiev", created.MiddleName);
        Assert.Equal("Petrov", created.LastName);
        Assert.Null(created.FirstNameLatin);
        Assert.Null(created.MiddleNameLatin);
        Assert.Null(created.LastNameLatin);
        Assert.Equal("9001154218", created.NationalId);
        Assert.Equal(new DateOnly(1990, 1, 15), created.DateOfBirth);
        Assert.Equal("PA1234567", created.PassportNumber);
        Assert.Equal(new DateOnly(2030, 5, 1), created.PassportExpiresOn);
        Assert.Equal("ivan.petrov@example.com", created.Email);
        Assert.Equal("+359888111222", created.PhoneNumber);
        Assert.Equal("BG", created.ResidenceCountryCode);
        Assert.Equal("Bulgaria", created.ResidenceCountryName);
        Assert.Equal("Test customer", created.Notes);

        var list = await GetCustomersAsync(client);
        Assert.Equal(1, list.Page);
        Assert.Equal(13, list.PageSize);
        Assert.Equal(1, list.TotalCount);
        Assert.Equal(1, list.TotalPages);
        var listCustomer = Assert.Single(list.Items);
        Assert.Equal(created.Id, listCustomer.Id);
        Assert.Equal("Ivan", listCustomer.FirstName);
        Assert.Equal("Petrov", listCustomer.LastName);
        Assert.Equal("ivan.petrov@example.com", listCustomer.Email);
        Assert.Equal("+359888111222", listCustomer.PhoneNumber);
        Assert.Equal(new DateOnly(1990, 1, 15), listCustomer.DateOfBirth);
        Assert.Equal(CalculateExpectedAge(new DateOnly(1990, 1, 15), DateOnly.FromDateTime(DateTime.UtcNow)), listCustomer.Age);
        Assert.Equal(new DateOnly(2030, 5, 1), listCustomer.PassportExpiresOn);
        Assert.True(listCustomer.IsPassportValid);

        var details = await client.GetFromJsonAsync<CustomerDetailsResponse>(
            $"/api/customers/{created.Id}",
            TestCancellationToken);
        Assert.NotNull(details);
        Assert.Equal(created.Id, details.Id);
        Assert.Equal("9001154218", details.NationalId);

        var updateResponse = await client.PutAsJsonAsync($"/api/customers/{created.Id}",
            new UpdateCustomerRequest(
                "Ivan",
                "Nikolov",
                "Petrov",
                "Ivan",
                "Nikolov",
                "Petrov",
                "9001154218",
                new DateOnly(1990, 1, 15),
                "PA1234567",
                new DateOnly(2030, 5, 1),
                "ivan.petrov@example.com",
                "+359888111222",
                "GB",
                "Updated notes"),
            TestCancellationToken);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<CustomerDetailsResponse>(
            TestCancellationToken);
        Assert.NotNull(updated);
        Assert.Equal("Nikolov", updated.MiddleName);
        Assert.Equal("Updated notes", updated.Notes);
        Assert.Equal("9001154218", updated.NationalId);
        Assert.Equal("GB", updated.ResidenceCountryCode);
        Assert.Equal("United Kingdom", updated.ResidenceCountryName);

        var deleteResponse = await client.DeleteAsync($"/api/customers/{created.Id}", TestCancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getDeletedResponse = await client.GetAsync($"/api/customers/{created.Id}", TestCancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);
        Assert.Equal("application/problem+json", getDeletedResponse.Content.Headers.ContentType?.MediaType);
        var notFoundProblem = await getDeletedResponse.Content.ReadFromJsonAsync<ProblemDetails>(
            TestCancellationToken);
        Assert.NotNull(notFoundProblem);
        Assert.Equal(404, notFoundProblem.Status);
        Assert.Equal("Customer not found", notFoundProblem.Title);
        Assert.True(notFoundProblem.Extensions.ContainsKey("code"));
        Assert.Equal("customer.not_found", notFoundProblem.Extensions["code"]!.ToString());
        Assert.True(notFoundProblem.Extensions.ContainsKey("traceId"));
        Assert.False(string.IsNullOrWhiteSpace(notFoundProblem.Extensions["traceId"]!.ToString()));

        var listAfterDelete = await GetCustomersAsync(client);
        Assert.Empty(listAfterDelete.Items);
        Assert.Equal(0, listAfterDelete.TotalCount);
        Assert.Equal(0, listAfterDelete.TotalPages);
    }
}
