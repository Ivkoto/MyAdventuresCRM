using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SuiteCase.Server.Data;
using SuiteCase.Server.Features.Customers.DTO;

namespace SuiteCase.IntegrationTests.Customers;

[Collection(SqlServerCollection.Name)]
public sealed class CustomerUpdateEndpointTests(SqlServerFixture sqlServer)
    : CustomerEndpointTestBase(sqlServer)
{
    [Fact]
    public async Task UpdateCustomer_ReturnsConflict_WhenNationalIdBelongsToActiveCustomer()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var firstCustomer = await CreateCustomerAsync(client, CreateRequest(
            nationalId: "9001154218",
            passportNumber: "PA1234567"));

        var secondCustomer = await CreateCustomerAsync(client, CreateRequest(
            firstName: "Petar",
            lastName: "Ivanov",
            nationalId: "8507120055",
            passportNumber: "PB6543210"));

        var response = await client.PutAsJsonAsync($"/api/customers/{secondCustomer.Id}",
            new UpdateCustomerRequest(
                secondCustomer.FirstName,
                secondCustomer.MiddleName,
                secondCustomer.LastName,
                secondCustomer.FirstNameLatin,
                secondCustomer.MiddleNameLatin,
                secondCustomer.LastNameLatin,
                firstCustomer.NationalId,
                secondCustomer.DateOfBirth,
                secondCustomer.PassportNumber,
                secondCustomer.PassportExpiresOn,
                secondCustomer.Email,
                secondCustomer.PhoneNumber,
                secondCustomer.ResidenceCountryCode,
                secondCustomer.Notes));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal("Duplicate customer", problem.Title);
        Assert.Equal("A customer with this national ID already exists.", problem.Detail);
        Assert.Equal("customer.duplicate_national_id", problem.Extensions["code"]!.ToString());
        Assert.Equal(firstCustomer.Id, ((JsonElement)problem.Extensions["existingCustomerId"]!).GetInt32());
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsConflict_WhenPassportNumberBelongsToActiveCustomer()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var firstCustomer = await CreateCustomerAsync(client, CreateRequest(
            nationalId: "9001154218",
            passportNumber: "PA1234567"));

        var secondCustomer = await CreateCustomerAsync(client, CreateRequest(
            firstName: "Petar",
            lastName: "Ivanov",
            nationalId: "8507120055",
            passportNumber: "PB6543210"));

        var response = await client.PutAsJsonAsync($"/api/customers/{secondCustomer.Id}",
            new UpdateCustomerRequest(
                secondCustomer.FirstName,
                secondCustomer.MiddleName,
                secondCustomer.LastName,
                secondCustomer.FirstNameLatin,
                secondCustomer.MiddleNameLatin,
                secondCustomer.LastNameLatin,
                secondCustomer.NationalId,
                secondCustomer.DateOfBirth,
                $" {firstCustomer.PassportNumber!.ToLowerInvariant()} ",
                secondCustomer.PassportExpiresOn,
                secondCustomer.Email,
                secondCustomer.PhoneNumber,
                secondCustomer.ResidenceCountryCode,
                secondCustomer.Notes));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal("Duplicate customer", problem.Title);
        Assert.Equal("A customer with this passport number already exists.", problem.Detail);
        Assert.Equal("customer.duplicate_passport_number", problem.Extensions["code"]!.ToString());
        Assert.Equal(firstCustomer.Id, ((JsonElement)problem.Extensions["existingCustomerId"]!).GetInt32());
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenResidenceCountryCodeIsUnsupported()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var created = await CreateCustomerAsync(client, CreateRequest(
            nationalId: "9001154218",
            passportNumber: "PA1234567"));

        var response = await client.PutAsJsonAsync($"/api/customers/{created.Id}",
            new UpdateCustomerRequest(
                created.FirstName,
                created.MiddleName,
                created.LastName,
                created.FirstNameLatin,
                created.MiddleNameLatin,
                created.LastNameLatin,
                created.NationalId,
                created.DateOfBirth,
                created.PassportNumber,
                created.PassportExpiresOn,
                created.Email,
                created.PhoneNumber,
                "ZZ",
                created.Notes));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.True(problem.Errors.ContainsKey(nameof(CreateCustomerRequest.ResidenceCountryCode)));
        Assert.Contains(
            "Residence country code must be a supported European ISO alpha-2 country code.",
            problem.Errors[nameof(CreateCustomerRequest.ResidenceCountryCode)]);
    }

    [Fact]
    public async Task UpdateCustomer_DerivesDateOfBirthFromNationalId_WhenDateOfBirthIsMissing()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var created = await CreateCustomerAsync(client, CreateRequest(
            nationalId: "9001154218",
            passportNumber: "PA1234567",
            dateOfBirth: new DateOnly(1990, 1, 15)));

        var response = await client.PutAsJsonAsync($"/api/customers/{created.Id}",
            new UpdateCustomerRequest(
                created.FirstName,
                created.MiddleName,
                created.LastName,
                created.FirstNameLatin,
                created.MiddleNameLatin,
                created.LastNameLatin,
                "8501014017",
                null,
                created.PassportNumber,
                created.PassportExpiresOn,
                created.Email,
                created.PhoneNumber,
                created.ResidenceCountryCode,
                created.Notes));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<CustomerDetailsResponse>();
        Assert.NotNull(updated);
        Assert.Equal(new DateOnly(1985, 1, 1), updated.DateOfBirth);
        Assert.Equal("8501014017", updated.NationalId);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenDateOfBirthDoesNotMatchNationalId()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var created = await CreateCustomerAsync(client, CreateRequest(
            nationalId: "8501014017",
            passportNumber: "PA1234567",
            dateOfBirth: new DateOnly(1985, 1, 1)));

        var response = await client.PutAsJsonAsync($"/api/customers/{created.Id}",
            new UpdateCustomerRequest(
                created.FirstName,
                created.MiddleName,
                created.LastName,
                created.FirstNameLatin,
                created.MiddleNameLatin,
                created.LastNameLatin,
                created.NationalId,
                new DateOnly(1990, 6, 15),
                created.PassportNumber,
                created.PassportExpiresOn,
                created.Email,
                created.PhoneNumber,
                created.ResidenceCountryCode,
                created.Notes));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.True(problem.Errors.ContainsKey(nameof(CreateCustomerRequest.DateOfBirth)));
        Assert.Contains(
            "Date of birth must match the date encoded in a valid Bulgarian national ID.",
            problem.Errors[nameof(CreateCustomerRequest.DateOfBirth)]);

        var unchanged = await client.GetFromJsonAsync<CustomerDetailsResponse>($"/api/customers/{created.Id}");
        Assert.NotNull(unchanged);
        Assert.Equal(new DateOnly(1985, 1, 1), unchanged.DateOfBirth);
        Assert.Equal("8501014017", unchanged.NationalId);
    }

    [Fact]
    public async Task UpdateCustomer_ClearsSensitiveValues_WhenValuesAreRemoved()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var created = await CreateCustomerAsync(client, CreateRequest(
            nationalId: "9001154218",
            passportNumber: "PA1234567"));

        var response = await client.PutAsJsonAsync($"/api/customers/{created.Id}",
            new UpdateCustomerRequest(
                created.FirstName,
                created.MiddleName,
                created.LastName,
                created.FirstNameLatin,
                created.MiddleNameLatin,
                created.LastNameLatin,
                null,
                created.DateOfBirth,
                null,
                created.PassportExpiresOn,
                created.Email,
                created.PhoneNumber,
                created.ResidenceCountryCode,
                created.Notes));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<CustomerDetailsResponse>();
        Assert.NotNull(updated);
        Assert.Null(updated.NationalId);
        Assert.Null(updated.PassportNumber);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SuiteCaseDbContext>();
        var customer = await db.Customers
            .IgnoreQueryFilters()
            .SingleAsync(c => c.Id == created.Id);

        Assert.Null(customer.NationalIdEncrypted);
        Assert.Null(customer.NationalIdHash);
        Assert.Null(customer.PassportNumberEncrypted);
        Assert.Null(customer.PassportNumberHash);
    }

    [Fact]
    public async Task UpdateCustomer_NormalizesTextSensitiveValuesAndCountryCode()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var created = await CreateCustomerAsync(client, CreateRequest());

        var response = await client.PutAsJsonAsync($"/api/customers/{created.Id}",
            new UpdateCustomerRequest(
                " Petar ",
                " Nikolov ",
                " Ivanov ",
                " Petar ",
                " Nikolov ",
                " Ivanov ",
                created.NationalId,
                created.DateOfBirth,
                " pa1234567 ",
                created.PassportExpiresOn,
                created.Email,
                " +359888333444 ",
                "gb",
                " Updated notes "));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<CustomerDetailsResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Petar", updated.FirstName);
        Assert.Equal("Nikolov", updated.MiddleName);
        Assert.Equal("Ivanov", updated.LastName);
        Assert.Equal("Petar", updated.FirstNameLatin);
        Assert.Equal("Nikolov", updated.MiddleNameLatin);
        Assert.Equal("Ivanov", updated.LastNameLatin);
        Assert.Equal("PA1234567", updated.PassportNumber);
        Assert.Equal("+359888333444", updated.PhoneNumber);
        Assert.Equal("GB", updated.ResidenceCountryCode);
        Assert.Equal("United Kingdom", updated.ResidenceCountryName);
        Assert.Equal("Updated notes", updated.Notes);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsProblemDetails404_WhenCustomerDoesNotExist()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.PutAsJsonAsync("/api/customers/99999",
            new UpdateCustomerRequest(
                "Ivan", null, "Petrov", null, null, null,
                null, new DateOnly(1990, 1, 15), null, null, null, null, null, null));

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
