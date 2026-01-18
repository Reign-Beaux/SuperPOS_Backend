using Application.Interfaces.Persistence;
using Application.Interfaces.Persistence.Context;
using Application.Specifications;
using Infrastructure.Persistence.Specification;
using System.Linq.Expressions;

namespace Infrastructure.Persistence;

public sealed class Repository<T>(ISuperPOSDbContext context) : IRepository<T> where T : class
{
    private readonly ISuperPOSDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.Set<T>().AsNoTracking();
        query = query.Where(e => EF.Property<DateTime?>(e, "DeletedAt") == null);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<T>().AsNoTracking();
        query = query.Where(e =>
            EF.Property<Guid>(e, "Id") == id &&
            EF.Property<DateTime?>(e, "DeletedAt") == null);
        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public void Add(T entity) => _context.Set<T>().Add(entity);

    public void Update(T entity) => _context.Set<T>().Update(entity);

    public void Delete(T entity) => _context.Set<T>().Remove(entity);

    public async Task<IReadOnlyList<T>> QueryAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _context.Set<T>().AsNoTracking();

        query = query.Where(e => EF.Property<DateTime?>(e, "DeletedAt") == null);

        if (predicate != null)
            query = query.Where(predicate);
        if (orderBy != null)
            query = orderBy(query);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<T?> FirstOrDefaultAsync(
    Expression<Func<T, bool>> predicate,
    CancellationToken cancellationToken = default)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var query = _context.Set<T>().AsNoTracking();

        query = query
            .Where(e => EF.Property<DateTime?>(e, "DeletedAt") == null)
            .Where(predicate);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
    ISpecification<T> spec,
    CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(spec);
        query = query.Where(e => EF.Property<DateTime?>(e, "DeletedAt") == null);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(
    ISpecification<T> spec,
    CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(spec);
        query = query.Where(e => EF.Property<DateTime?>(e, "DeletedAt") == null);
        return await query.ToListAsync(cancellationToken);
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        => SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable().AsNoTracking(), spec);
}
