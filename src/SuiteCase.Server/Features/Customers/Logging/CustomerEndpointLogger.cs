namespace SuiteCase.Server.Features.Customers.Logging;

internal static partial class CustomerEndpointLogger
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "Customer {CustomerId} created")]
    internal static partial void CustomerCreated(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Warning,
        Message = "Customer create rejected: duplicate national ID detected")]
    internal static partial void CustomerCreateRejectedDuplicateNationalId(ILogger logger);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Warning,
        Message = "Customer create rejected: duplicate passport number detected")]
    internal static partial void CustomerCreateRejectedDuplicatePassportNumber(ILogger logger);

    [LoggerMessage(EventId = 1006, Level = LogLevel.Warning,
        Message = "Customer create: {ConflictKind} unique constraint race conflict during save")]
    internal static partial void CustomerCreateUniqueConstraintRaceConflict(ILogger logger, SensitiveIdentifierConflictKind conflictKind, Exception exception);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information,
        Message = "Customer {CustomerId} updated")]
    internal static partial void CustomerUpdated(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1007, Level = LogLevel.Warning,
        Message = "Customer {CustomerId} update rejected: duplicate national ID detected")]
    internal static partial void CustomerUpdateRejectedDuplicateNationalId(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1008, Level = LogLevel.Warning,
        Message = "Customer {CustomerId} update rejected: duplicate passport number detected")]
    internal static partial void CustomerUpdateRejectedDuplicatePassportNumber(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1009, Level = LogLevel.Warning,
        Message = "Customer {CustomerId} update: {ConflictKind} unique constraint race conflict during save")]
    internal static partial void CustomerUpdateUniqueConstraintRaceConflict(ILogger logger, int customerId, SensitiveIdentifierConflictKind conflictKind, Exception exception);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information,
        Message = "Customer {CustomerId} soft-deleted")]
    internal static partial void CustomerSoftDeleted(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1000, Level = LogLevel.Warning,
        Message = "Customer {CustomerId} not found on details request")]
    internal static partial void CustomerNotFoundOnDetailsRequest(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1010, Level = LogLevel.Warning,
        Message = "Customer {CustomerId} not found on update request")]
    internal static partial void CustomerNotFoundOnUpdateRequest(ILogger logger, int customerId);

    [LoggerMessage(EventId = 1011, Level = LogLevel.Warning,
        Message = "Customer {CustomerId} not found on delete request")]
    internal static partial void CustomerNotFoundOnDeleteRequest(ILogger logger, int customerId);
}
