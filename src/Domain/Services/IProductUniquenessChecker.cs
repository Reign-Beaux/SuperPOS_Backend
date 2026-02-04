namespace Domain.Services;

/// <summary>
/// Domain service for checking product uniqueness constraints.
/// Implemented in Infrastructure layer to access persistence.
/// </summary>
public interface IProductUniquenessChecker
{
    /// <summary>
    /// Checks if a product name is unique in the system.
    /// </summary>
    /// <param name="name">Product name to check</param>
    /// <param name="excludeId">Optional product ID to exclude from the check (for updates)</param>
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a barcode is unique in the system.
    /// </summary>
    /// <param name="barcode">Barcode to check</param>
    /// <param name="excludeId">Optional product ID to exclude from the check (for updates)</param>
    Task<bool> IsBarcodeUniqueAsync(string barcode, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
