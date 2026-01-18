using System.Linq.Expressions;

namespace Application.Specifications;

public abstract class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>> Criteria { get; }
    public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnable { get; private set; }

    protected BaseSpecification()
    {
        Criteria = x => true;
    }

    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    protected void AddOrderBy(Expression<Func<T, object>> orderBy)
    {
        OrderBy = orderBy;
    }

    protected void AddOrderByDescending(Expression<Func<T, object>> orderBy)
    {
        OrderByDescending = orderBy;
    }

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnable = true;
    }

    protected void AddInclude(Expression<Func<T, object>> include)
    {
        Includes.Add(include);
    }
}
