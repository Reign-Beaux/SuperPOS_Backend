using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence;

/// <summary>
/// Base repository implementation with common CRUD operations.
/// Specific repositories should inherit from this class.
/// IMPORTANT: Only aggregate roots can be persisted through repositories.
/// </summary>
public class RepositoryBase<T> : IRepositoryBase<T> where T : class, IAggregateRoot
{
    protected readonly SuperPOSDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public RepositoryBase(SuperPOSDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Apply soft delete filter if entity inherits from BaseEntity
        if (typeof(BaseEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => EF.Property<DateTime?>(e, nameof(BaseEntity.DeletedAt)) == null);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Apply ID filter
        query = query.Where(e => EF.Property<Guid>(e, "Id") == id);

        // Apply soft delete filter if entity inherits from BaseEntity
        if (typeof(BaseEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => EF.Property<DateTime?>(e, nameof(BaseEntity.DeletedAt)) == null);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public virtual void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        // Perform soft delete if entity inherits from BaseEntity
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.DeletedAt = DateTime.UtcNow;
            baseEntity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual async Task<IReadOnlyList<T>> QueryAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Apply soft delete filter if entity inherits from BaseEntity
        if (typeof(BaseEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => EF.Property<DateTime?>(e, nameof(BaseEntity.DeletedAt)) == null);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Apply soft delete filter if entity inherits from BaseEntity
        if (typeof(BaseEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => EF.Property<DateTime?>(e, nameof(BaseEntity.DeletedAt)) == null);
        }

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Apply soft delete filter if entity inherits from BaseEntity
        if (typeof(BaseEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => EF.Property<DateTime?>(e, nameof(BaseEntity.DeletedAt)) == null);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Apply soft delete filter if entity inherits from BaseEntity
        if (typeof(BaseEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => EF.Property<DateTime?>(e, nameof(BaseEntity.DeletedAt)) == null);
        }

        return await query.AnyAsync(predicate, cancellationToken);
    }
}
