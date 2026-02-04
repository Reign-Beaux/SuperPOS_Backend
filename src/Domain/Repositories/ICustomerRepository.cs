using Domain.Entities.Customers;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Customer aggregate root.
/// Provides specific operations for customer management.
/// </summary>
public interface ICustomerRepository : IRepositoryBase<Customer>
{
    /// <summary>
    /// Checks if a customer with the given email already exists.
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeId">Optional customer ID to exclude from the check (for updates)</param>
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by their email address.
    /// </summary>
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches customers by name (partial match).
    /// </summary>
    Task<IReadOnlyList<Customer>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
}
