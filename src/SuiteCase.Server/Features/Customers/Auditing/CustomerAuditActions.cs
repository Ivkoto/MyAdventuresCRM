namespace SuiteCase.Server.Features.Customers.Auditing;

/// <summary>
/// Defines stable audit identifiers for customer operations.
/// </summary>
internal static class CustomerAuditActions
{
    internal const string EntityType = "Customer";
    internal const string Created = "customer.created";
    internal const string Updated = "customer.updated";
    internal const string SoftDeleted = "customer.soft-deleted";
}
