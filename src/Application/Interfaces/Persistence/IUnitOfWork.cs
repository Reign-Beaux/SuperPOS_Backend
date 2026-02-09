using Domain.Entities;
using Domain.Repositories;

namespace Application.Interfaces.Persistence;

/// <summary>
/// Unit of Work pattern interface for coordinating transactions across multiple repositories.
/// Provides access to specific repository instances and transaction management.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Specific repositories as properties for type-safe access
    IProductRepository Products { get; }
    ICustomerRepository Customers { get; }
    IUserRepository Users { get; }
    ISaleRepository Sales { get; }
    IInventoryRepository Inventories { get; }
    IRoleRepository Roles { get; }
    ICashRegisterRepository CashRegisters { get; }

    /// <summary>
    /// Generic repository accessor for aggregate roots that don't need specialized operations.
    /// IMPORTANT: Only aggregate roots can be accessed through repositories.
    /// Internal entities (like SaleDetail, UserRole) must be accessed through their aggregate roots.
    /// For main aggregates with specialized methods, prefer using the specific repository properties above.
    /// </summary>
    IRepositoryBase<T> Repository<T>() where T : class, IAggregateRoot;

    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    /// <returns>The number of entities written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync();
}
