using Application.Specifications;

namespace Application.Interfaces.Persistence;

public interface IRepository<T>
{
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(T article);
    void Update(T article);
    void Delete(T article);
    Task<IReadOnlyList<T>> QueryAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllWithSpecAsync(
        ISpecification<T> spec,
        CancellationToken cancellationToken = default);
}
