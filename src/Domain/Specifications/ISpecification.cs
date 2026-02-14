using System.Linq.Expressions;

namespace Domain.Specifications;

/// <summary>
/// Specification pattern interface for building complex queries with filtering, sorting, paging, and eager loading.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the filter criteria (WHERE clause)
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Gets the list of navigation properties to include (eager loading)
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Gets the list of string-based navigation paths to include (for deep navigation)
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Gets the primary ORDER BY expression
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Gets the primary ORDER BY DESC expression
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Gets the list of secondary ordering expressions (THEN BY)
    /// </summary>
    List<Expression<Func<T, object>>> ThenByList { get; }

    /// <summary>
    /// Gets the list of secondary ordering expressions (THEN BY DESC)
    /// </summary>
    List<Expression<Func<T, object>>> ThenByDescendingList { get; }

    /// <summary>
    /// Gets the number of records to take (LIMIT)
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Gets the number of records to skip (OFFSET)
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Gets whether paging is enabled
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Gets whether to use AsNoTracking (default: true for read-only queries)
    /// </summary>
    bool AsNoTracking { get; }

    /// <summary>
    /// Gets whether to use AsSplitQuery (default: true for multiple includes)
    /// </summary>
    bool AsSplitQuery { get; }
}
