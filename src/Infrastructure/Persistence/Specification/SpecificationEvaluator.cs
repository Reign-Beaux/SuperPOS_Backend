using Domain.Specifications;

namespace Infrastructure.Persistence.Specification;

/// <summary>
/// Evaluator that applies a specification to an IQueryable.
/// Handles filtering, ordering, paging, and eager loading.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public sealed class SpecificationEvaluator<T> where T : class
{
    /// <summary>
    /// Applies a specification to an IQueryable and returns the modified query
    /// </summary>
    /// <param name="inputQuery">The base query to apply the specification to</param>
    /// <param name="spec">The specification containing filtering, ordering, and paging rules</param>
    /// <returns>Modified IQueryable with all specification rules applied</returns>
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
    {
        var query = inputQuery;

        // 1. Apply filtering (WHERE clause)
        if (spec.Criteria is not null)
        {
            query = query.Where(spec.Criteria);
        }

        // 2. Apply eager loading - Expression-based includes
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        // 3. Apply eager loading - String-based includes (for deep navigation)
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        // 4. Apply ordering (ORDER BY, THEN BY)
        if (spec.OrderBy is not null)
        {
            query = query.OrderBy(spec.OrderBy);

            // Apply ThenBy (secondary ordering)
            query = spec.ThenByList.Aggregate(
                (IOrderedQueryable<T>)query,
                (current, thenBy) => current.ThenBy(thenBy)
            );

            // Apply ThenByDescending (secondary descending ordering)
            query = spec.ThenByDescendingList.Aggregate(
                (IOrderedQueryable<T>)query,
                (current, thenByDesc) => current.ThenByDescending(thenByDesc)
            );
        }
        else if (spec.OrderByDescending is not null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);

            // Apply ThenBy (secondary ordering)
            query = spec.ThenByList.Aggregate(
                (IOrderedQueryable<T>)query,
                (current, thenBy) => current.ThenBy(thenBy)
            );

            // Apply ThenByDescending (secondary descending ordering)
            query = spec.ThenByDescendingList.Aggregate(
                (IOrderedQueryable<T>)query,
                (current, thenByDesc) => current.ThenByDescending(thenByDesc)
            );
        }

        // 5. Apply paging (SKIP/TAKE) - MUST be after ordering
        if (spec.IsPagingEnabled)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }

        // 6. Apply query optimizations
        if (spec.AsSplitQuery)
        {
            query = query.AsSplitQuery();
        }

        if (spec.AsNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }
}
