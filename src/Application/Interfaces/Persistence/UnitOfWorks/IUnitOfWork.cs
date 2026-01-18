namespace Application.Interfaces.Persistence.UnitOfWorks;

public interface IUnitOfWork : IDisposable
{
  IRepository<T> Repository<T>() where T : class;
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
  Task BeginTransactionAsync();
  Task CommitTransactionAsync(CancellationToken cancellationToken = default);
  Task RollbackTransactionAsync();
}
