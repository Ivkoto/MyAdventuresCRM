namespace SuiteCase.Server.Features.Customers;

internal sealed record SensitiveIdentifierConflict(SensitiveIdentifierConflictKind Kind, int ExistingCustomerId);