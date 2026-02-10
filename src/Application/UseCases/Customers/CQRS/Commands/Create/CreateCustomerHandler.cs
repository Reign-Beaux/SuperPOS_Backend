using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Entities.Customers;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.UseCases.Customers.CQRS.Commands.Create;

public sealed class CreateCustomerHandler
    : IRequestHandler<CreateCustomerCommand, OperationResult<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerUniquenessChecker _uniquenessChecker;

    public CreateCustomerHandler(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICustomerUniquenessChecker uniquenessChecker)
    {
        
        _unitOfWork = unitOfWork;
        _uniquenessChecker = uniquenessChecker;
    }

    public async Task<OperationResult<Guid>> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        // Validate email uniqueness if provided
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var isEmailUnique = await _uniquenessChecker.IsEmailUniqueAsync(
                request.Email,
                cancellationToken: cancellationToken);

            if (!isEmailUnique)
                return Result.Error(
                    ErrorResult.Exists,
                    detail: CustomerMessages.AlreadyExists.WithEmail(request.Email));
        }

        // Create value objects
        var name = PersonName.Create(request.Name, request.FirstLastname, request.SecondLastname);

        var email = string.IsNullOrWhiteSpace(request.Email)
            ? null
            : Email.Create(request.Email);

        var phone = string.IsNullOrWhiteSpace(request.Phone)
            ? null
            : PhoneNumber.Create(request.Phone);

        // Use domain factory method
        var customer = Customer.Create(name, email, phone, request.BirthDate);

        // Use specific repository
        _unitOfWork.Customers.Add(customer);

        var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (affectedRows == 0)
            return Result.Error(
                ErrorResult.BadRequest,
                detail: CustomerMessages.Create.Failed);

        

        return Result.Created(customer.Id);
    }
}
