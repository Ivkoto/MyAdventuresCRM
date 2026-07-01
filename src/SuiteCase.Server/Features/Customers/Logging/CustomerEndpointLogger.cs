namespace SuiteCase.Server.Features.Customers.Logging;

public static partial class CustomerEndpointLogger
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "Customer {CustomerId} created")]
    public static partial void CustomerCreated(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Warning,
        Message = "Customer create rejected: duplicate national ID detected")]
    public static partial void CustomerCreateRejectedDuplicateNationalId(ILogger logger);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Warning,
        Message = "Customer create rejected: duplicate passport number detected")]
    public static partial void CustomerCreateRejectedDuplicatePassportNumber(ILogger logger);

    [LoggerMessage(EventId = 1006, Level = LogLevel.Warning,
        Message = "Customer create: unique constraint race conflict during save")]
    public static partial void CustomerCreateUniqueConstraintRaceConflict(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information,
        Message = "Customer {CustomerId} updated")]
    public static partial void CustomerUpdated(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1007, Level = LogLevel.Warning,
        Message = "Customer {CustomerId} update rejected: duplicate national ID detected")]
    public static partial void CustomerUpdateRejectedDuplicateNationalId(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1008, Level = LogLevel.Warning,
        Message = "Customer {CustomerId} update rejected: duplicate passport number detected")]
    public static partial void CustomerUpdateRejectedDuplicatePassportNumber(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1009, Level = LogLevel.Warning,
        Message = "Customer {CustomerId} update: unique constraint race conflict during save")]
    public static partial void CustomerUpdateUniqueConstraintRaceConflict(ILogger logger, int customerId, Exception exception);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information,
        Message = "Customer {CustomerId} soft-deleted")]
    public static partial void CustomerSoftDeleted(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1000, Level = LogLevel.Warning,
        Message = "Customer {CustomerId} not found on details request")]
    public static partial void CustomerNotFoundOnDetailsRequest(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1010, Level = LogLevel.Warning,
        Message = "Customer {CustomerId} not found on update request")]
    public static partial void CustomerNotFoundOnUpdateRequest(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1011, Level = LogLevel.Warning,
        Message = "Customer {CustomerId} not found on delete request")]
    public static partial void CustomerNotFoundOnDeleteRequest(ILogger logger, int customerId);
}
