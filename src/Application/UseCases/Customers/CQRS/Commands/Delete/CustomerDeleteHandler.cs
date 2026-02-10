using Domain.Entities.Customers;
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;

namespace Application.UseCases.Customers.CQRS.Commands.Delete;

public sealed class CustomerDeleteHandler
    : IRequestHandler<CustomerDeleteCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerDeleteHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<VoidResult>> Handle(
        CustomerDeleteCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id, cancellationToken);

        if (customer is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: CustomerMessages.NotFound.WithId(request.Id.ToString()));
        }

        _unitOfWork.Customers.Delete(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
