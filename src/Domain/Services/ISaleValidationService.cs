namespace Domain.Services;

/// <summary>
/// Domain service for validating sale prerequisites.
/// Ensures that referenced entities (Customer, User) exist before creating a sale.
/// </summary>
public interface ISaleValidationService
{
    /// <summary>
    /// Validates that a customer exists and is not deleted.
    /// </summary>
    Task<bool> CustomerExistsAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a user exists and is not deleted.
    /// </summary>
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a product exists and is not deleted.
    /// </summary>
    Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken = default);
}
