using Application.Interfaces.Persistence;
using Application.Interfaces.Persistence.UnitOfWorks;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence;

public class UnitOfWork(SuperPOSDbContext context) : IUnitOfWork
{
    private readonly SuperPOSDbContext _context = context;
    private IDbContextTransaction? _transaction;
    private readonly Dictionary<Type, object> _repositories = [];
    private bool _disposed = false;

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        if (!_repositories.TryGetValue(type, out object? value))
        {
            var repoInstance = new Repository<T>(_context);
            value = repoInstance;
            _repositories[type] = value;
        }

        return (IRepository<T>)value;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
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