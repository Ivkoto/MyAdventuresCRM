using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SuiteCase.Server.Features.Customers.DTO;

namespace SuiteCase.IntegrationTests.Customers;

[Collection(SqlServerCollection.Name)]
public sealed class CustomerReadEndpointTests(SqlServerFixture sqlServer)
    : CustomerEndpointTestBase(sqlServer)
{
    [Fact]
    public async Task GetCustomers_ReturnsPassportValidityFalse_WhenPassportExpiresBeforeSixMonthRule()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var passportExpiresBeforeSixMonths = today.AddMonths(6).AddDays(-1);

        var response = await client.PostAsJsonAsync("/api/customers",
            CreateRequest(
                nationalId: "8507120055",
                passportNumber: "PB6543210",
                passportExpiresOn: passportExpiresBeforeSixMonths));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var list = await GetCustomersAsync(client);
        Assert.Equal(1, list.Page);
        Assert.Equal(13, list.PageSize);
        Assert.Equal(1, list.TotalCount);
        Assert.Equal(1, list.TotalPages);
        var listCustomer = Assert.Single(list.Items);
        Assert.Equal(passportExpiresBeforeSixMonths, listCustomer.PassportExpiresOn);
        Assert.False(listCustomer.IsPassportValid);
    }

    [Fact]
    public async Task GetCustomers_ReturnsRequestedPageWithStableNameOrderingAndMetadata()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var customers = new[]
        {
            ("Anna", "Zeta", "1000000001", "PA00001"),
            ("Anna", "Alpha", "1000000002", "PA00002"),
            ("Boris", "Beta", "1000000003", "PA00003"),
            ("Daniel", "Delta", "1000000004", "PA00004"),
            ("Elena", "Epsilon", "1000000005", "PA00005")
        };

        foreach (var (firstName, lastName, nationalId, passportNumber) in customers)
        {
            await CreateCustomerAsync(client, CreateRequest(
                firstName: firstName,
                middleName: null,
                lastName: lastName,
                nationalId: nationalId,
                passportNumber: passportNumber));
        }

        var firstPage = await GetCustomersAsync(client, "?page=1&pageSize=2");
        var secondPage = await GetCustomersAsync(client, "?page=2&pageSize=2");
        var thirdPage = await GetCustomersAsync(client, "?page=3&pageSize=2");

        Assert.Equal(5, firstPage.TotalCount);
        Assert.Equal(3, firstPage.TotalPages);
        Assert.Equal(1, firstPage.Page);
        Assert.Equal(2, firstPage.PageSize);
        Assert.Collection(
            firstPage.Items,
            customer => Assert.Equal(("Anna", "Alpha"), (customer.FirstName, customer.LastName)),
            customer => Assert.Equal(("Anna", "Zeta"), (customer.FirstName, customer.LastName)));

        Assert.Equal(2, secondPage.Page);
        Assert.Collection(
            secondPage.Items,
            customer => Assert.Equal(("Boris", "Beta"), (customer.FirstName, customer.LastName)),
            customer => Assert.Equal(("Daniel", "Delta"), (customer.FirstName, customer.LastName)));

        Assert.Equal(3, thirdPage.Page);
        var finalCustomer = Assert.Single(thirdPage.Items);
        Assert.Equal(("Elena", "Epsilon"), (finalCustomer.FirstName, finalCustomer.LastName));
    }

    [Fact]
    public async Task GetCustomers_ReturnsEmptyPageWithMetadata_WhenRequestedPageIsAfterLastPage()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        await CreateCustomerAsync(client, CreateRequest(
            nationalId: "1000000031",
            passportNumber: "PD00031"));

        var result = await GetCustomersAsync(client, "?page=3&pageSize=2");

        Assert.Equal(3, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetCustomers_UsesIdAsStableTieBreaker_WhenCustomersHaveTheSameName()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var firstCustomer = await CreateCustomerAsync(client, CreateRequest(
            firstName: "Ivan",
            lastName: "Petrov",
            nationalId: "1000000032",
            passportNumber: "PD00032"));
        var secondCustomer = await CreateCustomerAsync(client, CreateRequest(
            firstName: "Ivan",
            lastName: "Petrov",
            nationalId: "1000000033",
            passportNumber: "PD00033"));

        var firstPage = await GetCustomersAsync(client, "?page=1&pageSize=1");
        var secondPage = await GetCustomersAsync(client, "?page=2&pageSize=1");

        Assert.Equal(firstCustomer.Id, Assert.Single(firstPage.Items).Id);
        Assert.Equal(secondCustomer.Id, Assert.Single(secondPage.Items).Id);
    }

    [Theory]
    [InlineData("?page=0&pageSize=13")]
    [InlineData("?page=1000001&pageSize=13")]
    [InlineData("?page=1&pageSize=0")]
    [InlineData("?page=1&pageSize=101")]
    [InlineData("?search=abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvw")]
    public async Task GetCustomers_ReturnsBadRequest_WhenPaginationParametersAreInvalid(string queryString)
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync($"/api/customers{queryString}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.NotEmpty(problem.Errors);
    }

    [Fact]
    public async Task GetCustomers_FindsCustomerByPartialBulgarianName()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var created = await CreateCustomerAsync(client, CreateRequest(
            firstName: "Иван",
            middleName: "Георгиев",
            lastName: "Петров",
            nationalId: "1000000010",
            passportNumber: "PB00010"));

        foreach (var searchTerm in new[] { "ива", "еор", "етро" })
        {
            var result = await GetCustomersAsync(
                client,
                $"?search={Uri.EscapeDataString(searchTerm)}");

            var customer = Assert.Single(result.Items);
            Assert.Equal(created.Id, customer.Id);
        }
    }

    [Fact]
    public async Task GetCustomers_FindsCustomerByPartialLatinName()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var created = await CreateCustomerAsync(client, CreateRequest(
            firstName: "Александър",
            middleName: "Георгиев",
            lastName: "Димитров",
            nationalId: "1000000011",
            passportNumber: "PB00011"));

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/customers/{created.Id}",
            new UpdateCustomerRequest(
                created.FirstName,
                created.MiddleName,
                created.LastName,
                "Aleksandar",
                "Georgiev",
                "Dimitrov",
                created.NationalId,
                created.DateOfBirth,
                created.PassportNumber,
                created.PassportExpiresOn,
                created.Email,
                created.PhoneNumber,
                created.ResidenceCountryCode,
                created.Notes));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        foreach (var searchTerm in new[] { "leks", "eorg", "imit" })
        {
            var result = await GetCustomersAsync(client, $"?search={searchTerm}");

            var customer = Assert.Single(result.Items);
            Assert.Equal(created.Id, customer.Id);
        }
    }

    [Fact]
    public async Task GetCustomers_FindsCustomerByPartialNormalizedPhoneNumber()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var created = await CreateCustomerAsync(client, CreateRequest(
            nationalId: "1000000012",
            passportNumber: "PB00012",
            phoneNumber: "(+359) 888-111-222"));

        var result = await GetCustomersAsync(client, "?search=888111");

        var customer = Assert.Single(result.Items);
        Assert.Equal(created.Id, customer.Id);
    }

    [Fact]
    public async Task GetCustomers_MatchesSensitiveIdentifiersOnlyByExactValue()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var created = await CreateCustomerAsync(client, CreateRequest(
            nationalId: "1234567890",
            passportNumber: "XY987654",
            email: null,
            phoneNumber: null));

        var nationalIdResult = await GetCustomersAsync(
            client,
            $"?search={Uri.EscapeDataString(" 1234567890 ")}");
        var passportResult = await GetCustomersAsync(
            client,
            $"?search={Uri.EscapeDataString(" xy987654 ")}");
        var partialNationalIdResult = await GetCustomersAsync(client, "?search=4567");
        var partialPassportResult = await GetCustomersAsync(client, "?search=9876");

        Assert.Equal(created.Id, Assert.Single(nationalIdResult.Items).Id);
        Assert.Equal(created.Id, Assert.Single(passportResult.Items).Id);
        Assert.Empty(partialNationalIdResult.Items);
        Assert.Empty(partialPassportResult.Items);
    }

    [Fact]
    public async Task GetCustomers_AppliesSearchBeforePagination()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var customers = new[]
        {
            ("Mila", "1000000021", "PC00021"),
            ("Mina", "1000000022", "PC00022"),
            ("Mira", "1000000023", "PC00023"),
            ("Zara", "1000000024", "PC00024")
        };

        foreach (var (firstName, nationalId, passportNumber) in customers)
        {
            await CreateCustomerAsync(client, CreateRequest(
                firstName: firstName,
                middleName: null,
                nationalId: nationalId,
                passportNumber: passportNumber));
        }

        var result = await GetCustomersAsync(client, "?page=2&pageSize=2&search=Mi");

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        var customer = Assert.Single(result.Items);
        Assert.Equal("Mira", customer.FirstName);
    }

    [Fact]
    public async Task GetCustomerById_ReturnsProblemDetails404_WhenCustomerDoesNotExist()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/api/customers/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
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
