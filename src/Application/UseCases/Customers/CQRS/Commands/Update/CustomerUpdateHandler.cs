using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Entities.Customers;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.UseCases.Customers.CQRS.Commands.Update;

public sealed class CustomerUpdateHandler
    : IRequestHandler<CustomerUpdateCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerUniquenessChecker _uniquenessChecker;

    public CustomerUpdateHandler(IUnitOfWork unitOfWork, ICustomerUniquenessChecker uniquenessChecker)
    {
        _unitOfWork = unitOfWork;
        _uniquenessChecker = uniquenessChecker;
    }

    public async Task<OperationResult<VoidResult>> Handle(
        CustomerUpdateCommand request,
        CancellationToken cancellationToken)
    {
        // Get customer using specific repository
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id, cancellationToken);

        if (customer is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: CustomerMessages.NotFound.WithId(request.Id.ToString()));
        }

        // Validate email uniqueness if changed
        if (customer.Email != request.Email && !string.IsNullOrWhiteSpace(request.Email))
        {
            var isEmailUnique = await _uniquenessChecker.IsEmailUniqueAsync(
                request.Email,
                excludeId: request.Id,
                cancellationToken);

            if (!isEmailUnique)
            {
                return Result.Error(
                    ErrorResult.Exists,
                    detail: CustomerMessages.AlreadyExists.WithEmail(request.Email));
            }
        }

        // Create value objects
        var name = PersonName.Create(request.Name, request.FirstLastname, request.SecondLastname);

        var email = string.IsNullOrWhiteSpace(request.Email)
            ? null
            : Email.Create(request.Email);

        var phone = string.IsNullOrWhiteSpace(request.Phone)
            ? null
            : PhoneNumber.Create(request.Phone);

        // Use domain methods to update customer
        customer.UpdateInfo(name, email, phone, request.BirthDate);

        // Use specific repository
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
