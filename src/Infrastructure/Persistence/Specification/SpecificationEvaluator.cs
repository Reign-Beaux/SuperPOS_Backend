using Application.Specifications;

namespace Infrastructure.Persistence.Specification;

public sealed class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(IQueryable<T> query, ISpecification<T> spec)
    {
        if (spec.Criteria is not null)
        {
            query = query.Where(spec.Criteria);
        }

        if (spec.OrderBy is not null)
        {
            query = query.OrderBy(spec.OrderBy);
        }

        if (spec.OrderByDescending is not null)
        {
            query = query.OrderBy(spec.OrderByDescending);
        }

        if (spec.IsPagingEnable)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }

        query =
            spec.Includes
            .Aggregate(query, (current, include) => current.Include(include))
            .AsSplitQuery()
            .AsNoTracking();

        return query;
    }
}
