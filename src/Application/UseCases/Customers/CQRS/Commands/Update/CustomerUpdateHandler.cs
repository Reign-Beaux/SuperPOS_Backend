using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Customers;

namespace Application.UseCases.Customers.CQRS.Commands.Update;

public sealed class CustomerUpdateHandler
    : IRequestHandler<CustomerUpdateCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly CustomerRules _customerRules;

    public CustomerUpdateHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _customerRules = new CustomerRules(unitOfWork);
    }

    public async Task<OperationResult<VoidResult>> Handle(
        CustomerUpdateCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRules.GetByIdAsync(request.Id, cancellationToken);

        if (customer is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: CustomerMessages.NotFound.WithId(request.Id.ToString()));
        }

        request.Adapt(customer);

        var validationResult = await _customerRules.EnsureUniquenessAsync(
            customer,
            isUpdate: true,
            cancellationToken);

        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        _unitOfWork.Repository<Customer>().Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
