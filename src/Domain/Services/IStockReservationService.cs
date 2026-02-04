using Domain.ValueObjects;

namespace Domain.Services;

/// <summary>
/// Domain service for managing stock reservations during sale creation.
/// Ensures stock availability and handles transactional stock updates.
/// </summary>
public interface IStockReservationService
{
    /// <summary>
    /// Validates that sufficient stock exists for all items and reserves it in memory.
    /// This is a two-phase operation: validate + reserve, then commit or rollback.
    /// </summary>
    /// <param name="items">List of (ProductId, Quantity) to reserve</param>
    /// <returns>Success true if all stock is available, false with error message otherwise</returns>
    Task<(bool Success, string ErrorMessage)> ValidateAndReserveStockAsync(
        List<(Guid ProductId, Quantity Quantity)> items,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the stock reservation by calling SaveChanges on the UnitOfWork.
    /// </summary>
    Task CommitReservationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the stock reservation by reverting changes without saving.
    /// </summary>
    Task RollbackReservationAsync();
}
