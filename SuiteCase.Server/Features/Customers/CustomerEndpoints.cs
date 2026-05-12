using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SuiteCase.Core.Security;
using SuiteCase.Server.Data;

namespace SuiteCase.Server.Features.Customers;

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

        group.MapGet("", GetAllCustomers)
            .WithName(ListCustomersEndpointName)
            .Produces<List<CustomerListResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", GetCustomerById)
            .WithName(GetCustomerByIdEndpointName)
            .Produces<CustomerDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", CreateCustomer)
            .WithName(CreateCustomerEndpointName)
            .Produces<CustomerDetailsResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", UpdateCustomer)
            .WithName(UpdateCustomerEndpointName)
            .Produces<CustomerDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        group.MapDelete("/{id:int}", SoftDeleteCustomer)
            .WithName(SoftDeleteCustomerEndpointName)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<Ok<List<CustomerListResponse>>> GetAllCustomers(SuiteCaseDbContext db, CancellationToken ct)
    {
        var customers = await db.Customers
            .AsNoTracking()
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .Select(c => new CustomerListResponse(
                c.Id,
                c.FirstName,
                c.LastName,
                c.FirstNameLatin,
                c.LastNameLatin,
                c.DateOfBirth,
                c.PassportExpiresOn))
            .ToListAsync(ct);

        return TypedResults.Ok(customers);
    }

    private static async Task<IResult> GetCustomerById(int id, SuiteCaseDbContext db, ISensitiveDataProtector dataProtector, CancellationToken ct)
    {
        var customer = await db.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == id, ct);

        return customer is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(customer.ToCustomerDetailsResponse(dataProtector));
    }

    private static async Task<IResult> CreateCustomer(CreateCustomerRequest request, SuiteCaseDbContext db, ISensitiveDataProtector dataProtector, CancellationToken ct)
    {
        var nationalId = NormalizeSensitiveValue(request.NationalId);
        var passportNumber = NormalizeSensitiveValue(request.PassportNumber);

        var nationalIdHash = nationalId is null ? null : dataProtector.Hash(nationalId);
        var passportNumberHash = passportNumber is null ? null : dataProtector.Hash(passportNumber);

        if (nationalIdHash is not null && await db.Customers.AnyAsync(c => c.NationalIdHash == nationalIdHash, ct))
            return TypedResults.Conflict($"A customer with this national ID: {nationalId} already exist");

        if (passportNumberHash is not null && await db.Customers.AnyAsync(c => c.PassportNumberHash == passportNumberHash, ct))
            return TypedResults.Conflict($"A customer with this passport number: {passportNumber} already exist.");                 

        var customer = request.ToCustomer(DateTime.UtcNow);

        customer.SetNationalId(nationalId is null ? null : dataProtector.Protect(nationalId), nationalIdHash);
        customer.SetPassportNumber(passportNumber is null ? null : dataProtector.Protect(passportNumber), passportNumberHash);

        db.Customers.Add(customer);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when(IsUniqueConstraintViolation(ex))
        {
            return TypedResults.Conflict("A customer with the same national ID or passport number already exists.");
        }        

        return TypedResults.CreatedAtRoute(
            customer.ToCustomerDetailsResponse(dataProtector),
            GetCustomerByIdEndpointName,
            new { id = customer.Id }
        );
    }

    private static async Task<IResult> UpdateCustomer(int id, UpdateCustomerRequest request, SuiteCaseDbContext db, ISensitiveDataProtector dataProtector, CancellationToken ct)
    {
        var currentCustomer = await db.Customers.SingleOrDefaultAsync(c => c.Id == id, ct);

        if (currentCustomer is null)
            return TypedResults.NotFound();

        var nationalId = NormalizeSensitiveValue(request.NationalId);
        var passportNumber = NormalizeSensitiveValue(request.PassportNumber);

        var nationalIdHash = nationalId is null ? null : dataProtector.Hash(nationalId);
        var passportNumberHash = passportNumber is null ? null : dataProtector.Hash(passportNumber);

        if (nationalIdHash != currentCustomer.NationalIdHash)
        {
            if (nationalIdHash is not null && await db.Customers.AnyAsync(c => c.Id != id && c.NationalIdHash == nationalIdHash, ct))
                return TypedResults.Conflict($"A customer with this national ID: {nationalId} already exist.");

            currentCustomer.SetNationalId(nationalId is null ? null : dataProtector.Protect(nationalId), nationalIdHash);
        }

        if (passportNumberHash != currentCustomer.PassportNumberHash)
        {
            if (passportNumberHash is not null && await db.Customers.AnyAsync(c => c.Id != id && c.PassportNumberHash == passportNumberHash, ct))
                return TypedResults.Conflict($"A customer with this passport number: {passportNumber} already exist.");

            currentCustomer.SetPassportNumber(passportNumber is null ? null : dataProtector.Protect(passportNumber), passportNumberHash);
        }

        currentCustomer.UpdateFrom(request, DateTime.UtcNow);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return TypedResults.Conflict("A customer with the same national ID or passport number already exists.");
        }        

        return TypedResults.Ok(currentCustomer.ToCustomerDetailsResponse(dataProtector));
    }

    private static async Task<IResult> SoftDeleteCustomer(int id, SuiteCaseDbContext db, CancellationToken ct)
    {
        var currentCustomer = await db.Customers.SingleOrDefaultAsync(c => c.Id == id, ct);

        if (currentCustomer is null)
            return TypedResults.NotFound();

        currentCustomer.SoftDelete(DateTime.UtcNow);
        await db.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }

    private static string? NormalizeSensitiveValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        => exception.InnerException is SqlException sqlException && (sqlException.Number == 2601 || sqlException.Number == 2627);
    //2601 -> duplicate key row with unique index
    //2627 -> violation of unique constraint / primary key

}
