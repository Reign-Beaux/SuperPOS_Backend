using Domain.Repositories;
using Domain.Services;

namespace Infrastructure.Services.Domain;

/// <summary>
/// Implementation of sale validation service using repositories.
/// </summary>
public class SaleValidationService : ISaleValidationService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;

    public SaleValidationService(
        ICustomerRepository customerRepository,
        IUserRepository userRepository,
        IProductRepository productRepository)
    {
        _customerRepository = customerRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
    }

    public async Task<bool> CustomerExistsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        return customer != null;
    }

    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user != null;
    }

    public async Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        return product != null;
    }
}
