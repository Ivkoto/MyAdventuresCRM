using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuiteCase.Core.Entities;
using SuiteCase.Server.Auditing;
using SuiteCase.Server.Data;
using SuiteCase.Server.Features.Customers.DTO;
using SuiteCase.IntegrationTests.Customers;

namespace SuiteCase.IntegrationTests.Auditing;

[Collection(SqlServerCollection.Name)]
public sealed class CustomerAuditEventTests(SqlServerFixture sqlServer)
    : CustomerEndpointTestBase(sqlServer)
{
    private const string CustomerEntityType = "Customer";
    private const string CustomerCreatedAction = "customer.created";
    private const string CustomerUpdatedAction = "customer.updated";
    private const string CustomerSoftDeletedAction = "customer.soft-deleted";

    [Fact]
    public async Task CreateCustomer_WhenSuccessful_WritesCreatedAuditEvent()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);
        var startedAt = DateTimeOffset.UtcNow;

        var created = await CreateCustomerAsync(client, CreateRequest());

        var auditEvent = await GetSingleAuditEventAsync(factory, CustomerCreatedAction);
        AssertAuditEvent(auditEvent, CustomerCreatedAction, created.Id, startedAt);
        Assert.DoesNotContain("9001154218", auditEvent.Details ?? string.Empty);
        Assert.DoesNotContain("PA1234567", auditEvent.Details ?? string.Empty);
    }

    [Fact]
    public async Task UpdateCustomer_WhenSuccessful_WritesUpdatedAuditEvent()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);
        var created = await CreateCustomerAsync(client, CreateRequest());
        var startedAt = DateTimeOffset.UtcNow;

        var response = await client.PutAsJsonAsync(
            $"/api/customers/{created.Id}",
            CreateUpdateRequest(created, notes: "Updated notes"),
            TestCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auditEvent = await GetSingleAuditEventAsync(factory, CustomerUpdatedAction);
        AssertAuditEvent(auditEvent, CustomerUpdatedAction, created.Id, startedAt);
    }

    [Fact]
    public async Task SoftDeleteCustomer_WhenSuccessful_WritesSoftDeletedAuditEvent()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);
        var created = await CreateCustomerAsync(client, CreateRequest());
        var startedAt = DateTimeOffset.UtcNow;

        var response = await client.DeleteAsync($"/api/customers/{created.Id}", TestCancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var auditEvent = await GetSingleAuditEventAsync(factory, CustomerSoftDeletedAction);
        AssertAuditEvent(auditEvent, CustomerSoftDeletedAction, created.Id, startedAt);
    }

    [Fact]
    public async Task GetCustomerById_WhenSuccessful_DoesNotWriteAdditionalAuditEvent()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);
        var startedAt = DateTimeOffset.UtcNow;
        var created = await CreateCustomerAsync(client, CreateRequest());

        var response = await client.GetAsync($"/api/customers/{created.Id}", TestCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var details = await response.Content.ReadFromJsonAsync<CustomerDetailsResponse>(TestCancellationToken);
        Assert.NotNull(details);
        Assert.Equal("9001154218", details.NationalId);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SuiteCaseDbContext>();
        var auditEvent = Assert.Single(await db.AuditEvents.AsNoTracking().ToListAsync(TestCancellationToken));
        AssertAuditEvent(auditEvent, CustomerCreatedAction, created.Id, startedAt);
    }

    [Fact]
    public async Task CreateCustomer_WhenAuditWriterFails_RollsBackCustomerInsert()
    {
        using var factory = CreateFactoryWithFailingAuditWriter();
        using var client = CreateClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/customers",
            CreateRequest(),
            TestCancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SuiteCaseDbContext>();
        Assert.False(await db.Customers.IgnoreQueryFilters().AnyAsync(TestCancellationToken));
        Assert.False(await db.AuditEvents.AnyAsync(TestCancellationToken));
    }

    [Fact]
    public async Task CreateCustomer_WhenCommitAcknowledgementIsLost_DoesNotDuplicateWrites()
    {
        var interceptor = new CommitAcknowledgementFailureInterceptor();
        using var factory = CreateFactory(interceptor);
        using var client = CreateClient(factory);
        var request = CreateRequest(nationalId: null, passportNumber: null) with
        {
            PassportExpiresOn = null
        };
        var startedAt = DateTimeOffset.UtcNow;

        interceptor.FailNextCommit();

        var created = await CreateCustomerAsync(client, request);

        Assert.Equal(1, interceptor.FailureCount);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SuiteCaseDbContext>();
        Assert.Single(await db.Customers.IgnoreQueryFilters().AsNoTracking().ToListAsync(TestCancellationToken));

        var auditEvent = Assert.Single(await db.AuditEvents.AsNoTracking().ToListAsync(TestCancellationToken));
        AssertAuditEvent(auditEvent, CustomerCreatedAction, created.Id, startedAt);
    }

    [Fact]
    public async Task UpdateCustomer_WhenCommitAcknowledgementIsLost_DoesNotDuplicateAuditEvent()
    {
        var interceptor = new CommitAcknowledgementFailureInterceptor();
        using var factory = CreateFactory(interceptor);
        using var client = CreateClient(factory);
        var created = await CreateCustomerAsync(client, CreateRequest());
        var startedAt = DateTimeOffset.UtcNow;

        interceptor.FailNextCommit();

        var response = await client.PutAsJsonAsync(
            $"/api/customers/{created.Id}",
            CreateUpdateRequest(created, notes: "Updated after transient commit failure"),
            TestCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, interceptor.FailureCount);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SuiteCaseDbContext>();
        var currentCustomer = await db.Customers.AsNoTracking().SingleAsync(TestCancellationToken);
        Assert.Equal("Updated after transient commit failure", currentCustomer.Notes);

        var updatedAuditEvent = await db.AuditEvents
            .AsNoTracking()
            .SingleAsync(
                auditEvent => auditEvent.Action == CustomerUpdatedAction,
                TestCancellationToken);
        AssertAuditEvent(updatedAuditEvent, CustomerUpdatedAction, created.Id, startedAt);
    }

    private WebApplicationFactory<Program> CreateFactoryWithFailingAuditWriter()
        => CreateFactory(services =>
        {
            services.RemoveAll<IAuditEventWriter>();
            services.AddScoped<IAuditEventWriter>(_ => new ThrowingAuditEventWriter());
        });

    private static async Task<AuditEvent> GetSingleAuditEventAsync(
        WebApplicationFactory<Program> factory,
        string action)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SuiteCaseDbContext>();

        return await db.AuditEvents
            .AsNoTracking()
            .SingleAsync(auditEvent => auditEvent.Action == action, TestCancellationToken);
    }

    private static void AssertAuditEvent(
        AuditEvent auditEvent,
        string expectedAction,
        int expectedCustomerId,
        DateTimeOffset startedAt)
    {
        Assert.True(auditEvent.Id > 0);
        Assert.NotEqual(Guid.Empty, auditEvent.OperationId);
        Assert.Equal(expectedAction, auditEvent.Action);
        Assert.Equal(CustomerEntityType, auditEvent.EntityType);
        Assert.Equal(expectedCustomerId.ToString(CultureInfo.InvariantCulture), auditEvent.EntityId);
        Assert.Null(auditEvent.ActorId);
        Assert.False(string.IsNullOrWhiteSpace(auditEvent.CorrelationId));
        Assert.Null(auditEvent.Details);
        Assert.InRange(auditEvent.OccurredAt, startedAt, DateTimeOffset.UtcNow.AddSeconds(1));
    }

    private static UpdateCustomerRequest CreateUpdateRequest(
        CustomerDetailsResponse customer,
        string? notes)
        => new(
            customer.FirstName,
            customer.MiddleName,
            customer.LastName,
            customer.FirstNameLatin,
            customer.MiddleNameLatin,
            customer.LastNameLatin,
            customer.NationalId,
            customer.DateOfBirth,
            customer.PassportNumber,
            customer.PassportExpiresOn,
            customer.Email,
            customer.PhoneNumber,
            customer.ResidenceCountryCode,
            notes);

}
