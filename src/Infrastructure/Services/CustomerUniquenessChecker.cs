using Domain.Repositories;
using Domain.Services;

namespace Infrastructure.Services;

/// <summary>
/// Implementation of customer uniqueness checker using the repository.
/// </summary>
public class CustomerUniquenessChecker : ICustomerUniquenessChecker
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerUniquenessChecker(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return true; // Empty email is considered "unique" (allowed for optional emails)

        // Check if a customer with this email already exists
        var exists = await _customerRepository.ExistsByEmailAsync(email, excludeId, cancellationToken);

        // Return true if it does NOT exist (is unique)
        return !exists;
    }
}
