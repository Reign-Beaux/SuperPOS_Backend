using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Customers;

namespace Application.UseCases.Customers.CQRS.Commands.Delete;

public sealed class CustomerDeleteHandler
    : IRequestHandler<CustomerDeleteCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly CustomerRules _customerRules;

    public CustomerDeleteHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _customerRules = new CustomerRules(unitOfWork);
    }

    public async Task<OperationResult<VoidResult>> Handle(
        CustomerDeleteCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRules.GetByIdAsync(request.Id, cancellationToken);

        if (customer is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: CustomerMessages.NotFound.WithId(request.Id.ToString()));
        }

        _unitOfWork.Repository<Customer>().Delete(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
