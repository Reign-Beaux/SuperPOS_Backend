using Application.Events;
using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Repositories;

namespace Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation coordinating transactions across repositories.
/// Handles domain event dispatching after successful persistence.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly SuperPOSDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private IDbContextTransaction? _transaction;
    private readonly Dictionary<Type, object> _repositories = [];
    private bool _disposed = false;

    // Lazy-initialized specific repositories
    private IProductRepository? _products;
    private ICustomerRepository? _customers;
    private IUserRepository? _users;
    private ISaleRepository? _sales;
    private IInventoryRepository? _inventories;
    private IRoleRepository? _roles;

    public UnitOfWork(SuperPOSDbContext context, IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    // Specific repository properties with lazy initialization
    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public ISaleRepository Sales => _sales ??= new SaleRepository(_context);
    public IInventoryRepository Inventories => _inventories ??= new InventoryRepository(_context);
    public IRoleRepository Roles => _roles ??= new RoleRepository(_context);

    /// <summary>
    /// Generic repository accessor for aggregate roots that don't need specialized operations.
    /// IMPORTANT: Only aggregate roots can be accessed through repositories.
    /// For main aggregates with specialized methods, use the specific repository properties.
    /// </summary>
    public IRepositoryBase<T> Repository<T>() where T : class, IAggregateRoot
    {
        var type = typeof(T);

        if (!_repositories.TryGetValue(type, out var repository))
        {
            repository = new RepositoryBase<T>(_context);
            _repositories[type] = repository;
        }

        return (IRepositoryBase<T>)repository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events from all entities before saving
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear events from entities before saving to avoid duplicate dispatch
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        // Save changes to database
        var result = await _context.SaveChangesAsync(cancellationToken);

        // Dispatch events after successful persistence
        if (domainEvents.Any())
        {
            await _eventDispatcher.DispatchManyAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }

                _context.Dispose();
            }

            _disposed = true;
        }
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }
}
