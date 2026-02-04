using Domain.Repositories;
using Domain.Services;

namespace Infrastructure.Services.Domain;

/// <summary>
/// Implementation of user uniqueness checker using the repository.
/// </summary>
public class UserUniquenessChecker : IUserUniquenessChecker
{
    private readonly IUserRepository _userRepository;

    public UserUniquenessChecker(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false; // Email is required for users

        // Check if a user with this email already exists
        var exists = await _userRepository.ExistsByEmailAsync(email, excludeId, cancellationToken);

        // Return true if it does NOT exist (is unique)
        return !exists;
    }
}
