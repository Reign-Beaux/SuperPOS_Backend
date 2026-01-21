using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Customers;

namespace Application.UseCases.Customers;

internal sealed class CustomerRules(IUnitOfWork unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<OperationResult<VoidResult>> EnsureUniquenessAsync(
        Customer newCustomer,
        bool isUpdate,
        CancellationToken cancellationToken)
    {
        var newEmail = string.IsNullOrWhiteSpace(newCustomer.Email) ? null : newCustomer.Email;

        // Only validate if email is provided
        if (newEmail == null)
            return Result.Success();

        var currentCustomer = await _unitOfWork.Repository<Customer>().FirstOrDefaultAsync(
            predicate: isUpdate
                ? c => c.Id != newCustomer.Id && c.Email == newEmail
                : c => c.Email == newEmail,
            cancellationToken
        );

        if (currentCustomer is null)
            return Result.Success();

        if (!string.IsNullOrWhiteSpace(newEmail) && currentCustomer.Email == newEmail)
            return Result.Error(
                ErrorResult.Exists,
                detail: CustomerMessages.AlreadyExists.WithEmail(newEmail));

        return Result.Success();
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Customer>().GetByIdAsync(id, cancellationToken);
    }
}
