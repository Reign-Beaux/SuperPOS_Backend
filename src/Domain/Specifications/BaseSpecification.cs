using System.Linq.Expressions;

namespace Domain.Specifications;

/// <summary>
/// Base class for implementing the Specification pattern.
/// Provides fluent API for building complex queries with filtering, sorting, paging, and eager loading.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    public List<string> IncludeStrings { get; } = [];
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public List<Expression<Func<T, object>>> ThenByList { get; } = [];
    public List<Expression<Func<T, object>>> ThenByDescendingList { get; } = [];
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    public bool AsNoTracking { get; private set; } = true; // Default: true for read-only queries
    public bool AsSplitQuery { get; private set; } = true; // Default: true for multiple includes

    /// <summary>
    /// Creates a specification without criteria (returns all records)
    /// </summary>
    protected BaseSpecification()
    {
        Criteria = null;
    }

    /// <summary>
    /// Creates a specification with filter criteria
    /// </summary>
    /// <param name="criteria">Filter expression (WHERE clause)</param>
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <summary>
    /// Adds primary ordering (ORDER BY)
    /// </summary>
    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Adds primary descending ordering (ORDER BY DESC)
    /// </summary>
    protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }

    /// <summary>
    /// Adds secondary ordering (THEN BY)
    /// </summary>
    protected void AddThenBy(Expression<Func<T, object>> thenByExpression)
    {
        ThenByList.Add(thenByExpression);
    }

    /// <summary>
    /// Adds secondary descending ordering (THEN BY DESC)
    /// </summary>
    protected void AddThenByDescending(Expression<Func<T, object>> thenByDescExpression)
    {
        ThenByDescendingList.Add(thenByDescExpression);
    }

    /// <summary>
    /// Adds navigation property to include (eager loading)
    /// </summary>
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Adds string-based navigation path to include (for deep navigation like "Product.Category")
    /// </summary>
    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Enables paging with skip and take
    /// </summary>
    /// <param name="skip">Number of records to skip (OFFSET)</param>
    /// <param name="take">Number of records to take (LIMIT)</param>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    /// <summary>
    /// Enables or disables AsNoTracking (default: true)
    /// </summary>
    protected void SetTracking(bool enableTracking)
    {
        AsNoTracking = !enableTracking;
    }

    /// <summary>
    /// Enables or disables AsSplitQuery (default: true)
    /// </summary>
    protected void SetSplitQuery(bool enableSplitQuery)
    {
        AsSplitQuery = enableSplitQuery;
    }
}
