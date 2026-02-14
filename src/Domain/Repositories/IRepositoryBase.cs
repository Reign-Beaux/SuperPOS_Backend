using System.Linq.Expressions;
using Domain.Entities;
using Domain.Specifications;

namespace Domain.Repositories;

/// <summary>
/// Base interface with common operations for all repositories.
/// DO NOT use directly - each entity should have its own specific interface.
/// This interface provides shared CRUD operations that specific repositories can inherit.
/// IMPORTANT: Only aggregate roots can have repositories. Internal entities (like SaleDetail)
/// must be accessed through their aggregate root.
/// </summary>
/// <typeparam name="T">The entity type (must be an aggregate root)</typeparam>
public interface IRepositoryBase<T> where T : class, IAggregateRoot
{
    /// <summary>
    /// Gets all entities (excludes soft-deleted).
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity by its unique identifier.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// Call SaveChangesAsync on UnitOfWork to persist changes.
    /// </summary>
    void Add(T entity);

    /// <summary>
    /// Updates an existing entity.
    /// Call SaveChangesAsync on UnitOfWork to persist changes.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Soft deletes an entity (sets DeletedAt timestamp).
    /// Call SaveChangesAsync on UnitOfWork to persist changes.
    /// </summary>
    void Delete(T entity);

    /// <summary>
    /// Queries entities with optional predicate and ordering.
    /// </summary>
    Task<IReadOnlyList<T>> QueryAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the first entity matching the predicate, or null if not found.
    /// </summary>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the predicate.
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities matching a specification (with filtering, ordering, paging, and includes).
    /// Use this method for complex queries with multiple criteria, eager loading, and sorting.
    /// </summary>
    Task<IReadOnlyList<T>> ListAsync(
        ISpecification<T> spec,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching a specification (applies only the filter criteria, ignoring paging).
    /// Useful for getting total count when implementing pagination.
    /// </summary>
    Task<int> CountAsync(
        ISpecification<T> spec,
        CancellationToken cancellationToken = default);
}
