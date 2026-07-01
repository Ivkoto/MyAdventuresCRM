using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SuiteCase.Core.Helpers;
using SuiteCase.Core.Security;
using SuiteCase.Server.Common.DTO;
using SuiteCase.Server.Data;
using SuiteCase.Server.Data.ErrorHandling;
using SuiteCase.Server.Features.Customers.DTO;
using SuiteCase.Server.Features.Customers.ErrorHandling;
using SuiteCase.Server.Features.Customers.Helpers;
using SuiteCase.Server.Features.Customers.Logging;
using SuiteCase.Server.Security;

namespace SuiteCase.Server.Features.Customers;

internal sealed class CustomerEndpointLogs;

public static class CustomerEndpoints
{
    private const string ListCustomersEndpointName = "ListCustomers";
    private const string GetCustomerByIdEndpointName = "GetCustomerById";
    private const string CreateCustomerEndpointName = "CreateCustomer";
    private const string UpdateCustomerEndpointName = "UpdateCustomer";
    private const string SoftDeleteCustomerEndpointName = "SoftDeleteCustomer";

    public static RouteGroupBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers")
            .WithTags("Customers");

        group.MapGet("", GetCustomers)
            .WithName(ListCustomersEndpointName)
            .Produces<PagedResponse<CustomerShortDetailsResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        group.MapGet("/{id:int}", GetCustomerById)
            .WithName(GetCustomerByIdEndpointName)
            .Produces<CustomerDetailsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("", CreateCustomer)
            .WithName(CreateCustomerEndpointName)
            .Produces<CustomerDetailsResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", UpdateCustomer)
            .WithName(UpdateCustomerEndpointName)
            .Produces<CustomerDetailsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        group.MapDelete("/{id:int}", SoftDeleteCustomer)
            .WithName(SoftDeleteCustomerEndpointName)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<Ok<PagedResponse<CustomerShortDetailsResponse>>> GetCustomers(
        [AsParameters] CustomerQueryParameters parameters,
        SuiteCaseDbContext db,
        ISensitiveDataProtector dataProtector,
        CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = CustomerHelper.ApplySearch(db.Customers.AsNoTracking(), parameters.Search, dataProtector);
        var totalCount = await query.CountAsync(ct);

        var customers = await query
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ThenBy(c => c.Id)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(c => new
            {
                c.Id,
                c.FirstName,
                c.LastName,
                c.Email,
                c.PhoneNumber,
                c.DateOfBirth,
                c.PassportExpiresOn
            })
            .ToListAsync(ct);

        var response = customers
            .Select(c => new CustomerShortDetailsResponse(
                c.Id,
                c.FirstName,
                c.LastName,
                c.Email,
                c.PhoneNumber,
                c.DateOfBirth,
                CustomerHelper.CalculateAge(c.DateOfBirth, today),
                c.PassportExpiresOn,
                PassportHelper.IsValid(c.PassportExpiresOn, today)))
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

        return TypedResults.Ok(new PagedResponse<CustomerShortDetailsResponse>(
            response,
            parameters.Page,
            parameters.PageSize,
            totalCount,
            totalPages));
    }

    private static async Task<Results<Ok<CustomerDetailsResponse>, ProblemHttpResult>> GetCustomerById(
        int id,
        HttpContext httpContext,
        SuiteCaseDbContext db,
        ISensitiveDataProtector dataProtector,
        ILogger<CustomerEndpointLogs> logger,
        CancellationToken ct)
    {
        var customer = await db.Customers.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, ct);

        if (customer is null)
        {
            CustomerEndpointLogger.CustomerNotFoundOnDetailsRequest(logger, id);
            return CustomerProblems.NotFound(httpContext);
        }

        return TypedResults.Ok(customer.ToCustomerDetailsResponse(dataProtector));
    }

    private static async Task<Results<CreatedAtRoute<CustomerDetailsResponse>, ProblemHttpResult, ValidationProblem>> CreateCustomer(
        CreateCustomerRequest request,
        HttpContext httpContext,
        SuiteCaseDbContext db,
        ISensitiveDataProtector dataProtector,
        ILogger<CustomerEndpointLogs> logger,
        CancellationToken ct)
    {
        var isValidCountryCode = CustomerHelper.TryGetValidResidenceCountryCode(request.ResidenceCountryCode, out var residenceCountryCode);
        if (!isValidCountryCode)
            return CustomerProblems.InvalidResidenceCountryCode();

        var nationalId = request.NationalId.NormalizeSensitiveValue();
        var passportNumber = request.PassportNumber.NormalizeSensitiveValue();

        var nationalIdHash = nationalId is null ? null : dataProtector.Hash(nationalId);
        var passportNumberHash = passportNumber is null ? null : dataProtector.Hash(passportNumber);

        if (nationalIdHash is not null)
        {
            var existingCustomerId = await db.Customers
                .Where(c => c.NationalIdHash == nationalIdHash)
                .Select(c => (int?)c.Id)
                .SingleOrDefaultAsync(ct);

            if (existingCustomerId is not null)
            {
                CustomerEndpointLogger.CustomerCreateRejectedDuplicateNationalId(logger);
                return CustomerProblems.DuplicateNationalId(httpContext, existingCustomerId.Value);
            }
        }

        if (passportNumberHash is not null)
        {
            var existingCustomerId = await db.Customers
                .Where(c => c.PassportNumberHash == passportNumberHash)
                .Select(c => (int?)c.Id)
                .SingleOrDefaultAsync(ct);

            if (existingCustomerId is not null)
            {
                CustomerEndpointLogger.CustomerCreateRejectedDuplicatePassportNumber(logger);
                return CustomerProblems.DuplicatePassportNumber(httpContext, existingCustomerId.Value);
            }
        }

        var customer = request.ToCustomer(DateTimeOffset.UtcNow, nationalId, residenceCountryCode);

        customer.SetNationalId(nationalId is null ? null : dataProtector.Protect(nationalId), nationalIdHash);
        customer.SetPassportNumber(passportNumber is null ? null : dataProtector.Protect(passportNumber), passportNumberHash);

        db.Customers.Add(customer);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (DbExceptionsHelper.IsUniqueConstraintViolation(ex))
        {
            CustomerEndpointLogger.CustomerCreateUniqueConstraintRaceConflict(logger, ex);
            return CustomerProblems.DuplicateSensitiveIdentifier(httpContext);
        }

        CustomerEndpointLogger.CustomerCreated(logger, customer.Id);

        return TypedResults.CreatedAtRoute(
            customer.ToCustomerDetailsResponse(dataProtector),
            GetCustomerByIdEndpointName,
            new { id = customer.Id }
        );
    }

    private static async Task<Results<Ok<CustomerDetailsResponse>,ProblemHttpResult, ValidationProblem>> UpdateCustomer(
        int id,
        UpdateCustomerRequest request,
        HttpContext httpContext,
        SuiteCaseDbContext db,
        ISensitiveDataProtector dataProtector,
        ILogger<CustomerEndpointLogs> logger,
        CancellationToken ct)
    {
        var currentCustomer = await db.Customers.SingleOrDefaultAsync(c => c.Id == id, ct);

        if (currentCustomer is null)
        {
            CustomerEndpointLogger.CustomerNotFoundOnUpdateRequest(logger, id);
            return CustomerProblems.NotFound(httpContext);
        }

        var isValidCountryCode = CustomerHelper.TryGetValidResidenceCountryCode(request.ResidenceCountryCode, out var residenceCountryCode);
        if (!isValidCountryCode)
            return CustomerProblems.InvalidResidenceCountryCode();

        var nationalId = request.NationalId.NormalizeSensitiveValue();
        var passportNumber = request.PassportNumber.NormalizeSensitiveValue();
        var nationalIdHash = nationalId is null ? null : dataProtector.Hash(nationalId);
        var passportNumberHash = passportNumber is null ? null : dataProtector.Hash(passportNumber);

        if (nationalIdHash != currentCustomer.NationalIdHash)
        {
            if (nationalIdHash is not null)
            {
                var existingCustomerId = await db.Customers
                    .Where(c => c.Id != id && c.NationalIdHash == nationalIdHash)
                    .Select(c => (int?)c.Id)
                    .SingleOrDefaultAsync(ct);

                if (existingCustomerId is not null)
                {
                    CustomerEndpointLogger.CustomerUpdateRejectedDuplicateNationalId(logger, id);
                    return CustomerProblems.DuplicateNationalId(httpContext, existingCustomerId.Value);
                }
            }

            currentCustomer.SetNationalId(nationalId is null ? null : dataProtector.Protect(nationalId), nationalIdHash);
        }

        if (passportNumberHash != currentCustomer.PassportNumberHash)
        {
            if (passportNumberHash is not null)
            {
                var existingCustomerId = await db.Customers
                    .Where(c => c.Id != id && c.PassportNumberHash == passportNumberHash)
                    .Select(c => (int?)c.Id)
                    .SingleOrDefaultAsync(ct);

                if (existingCustomerId is not null)
                {
                    CustomerEndpointLogger.CustomerUpdateRejectedDuplicatePassportNumber(logger, id);
                    return CustomerProblems.DuplicatePassportNumber(httpContext, existingCustomerId.Value);
                }
            }

            currentCustomer.SetPassportNumber(passportNumber is null ? null : dataProtector.Protect(passportNumber), passportNumberHash);
        }

        currentCustomer.UpdateFrom(request, DateTimeOffset.UtcNow, nationalId, residenceCountryCode);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (DbExceptionsHelper.IsUniqueConstraintViolation(ex))
        {
            CustomerEndpointLogger.CustomerUpdateUniqueConstraintRaceConflict(logger, id, ex);
            return CustomerProblems.DuplicateSensitiveIdentifier(httpContext);
        }

        CustomerEndpointLogger.CustomerUpdated(logger, id);

        return TypedResults.Ok(currentCustomer.ToCustomerDetailsResponse(dataProtector));
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> SoftDeleteCustomer(
        int id,
        HttpContext httpContext,
        SuiteCaseDbContext db,
        ILogger<CustomerEndpointLogs> logger,
        CancellationToken ct)
    {
        var currentCustomer = await db.Customers.SingleOrDefaultAsync(c => c.Id == id, ct);

        if (currentCustomer is null)
        {
            CustomerEndpointLogger.CustomerNotFoundOnDeleteRequest(logger, id);
            return CustomerProblems.NotFound(httpContext);
        }

        currentCustomer.SoftDelete(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(ct);

        CustomerEndpointLogger.CustomerSoftDeleted(logger,id);

        return TypedResults.NoContent();
    }
}
