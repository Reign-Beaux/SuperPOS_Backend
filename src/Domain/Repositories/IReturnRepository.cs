using Domain.Entities.Returns;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Return entity.
/// </summary>
public interface IReturnRepository : IRepositoryBase<Return>
{
    /// <summary>
    /// Gets a return by ID with all related entities loaded.
    /// </summary>
    Task<Return?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all returns for a specific sale.
    /// </summary>
    Task<List<Return>> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all returns with a specific status.
    /// </summary>
    Task<List<Return>> GetByStatusAsync(ReturnStatus status, CancellationToken cancellationToken = default);
}
