using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace SuiteCase.Server.Data.ErrorHandling;

/// <summary>
/// Classifies EF Core update exceptions by inspecting the underlying SQL Server error.
/// </summary>
public static class SqlServerExceptionClassifier
{
    // 2601 -> duplicate key row with unique index
    private const int CannotInsertDuplicateKeyRow = 2601;
    // 2627 -> violation of unique constraint / primary key
    private const int ViolationOfUniqueConstraint = 2627;

    /// <summary>
    /// Determines whether a database update exception was caused by a SQL Server unique constraint violation.
    /// </summary>
    /// <param name="exception">The database update exception raised while saving database changes.</param>
    /// <returns>
    /// <see langword="true" /> for SQL Server duplicate key or unique constraint violations; otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsUniqueConstraintViolation(DbUpdateException exception)
        => exception.InnerException is SqlException sqlException
           && (sqlException.Number == CannotInsertDuplicateKeyRow || sqlException.Number == ViolationOfUniqueConstraint);
}
