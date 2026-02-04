using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;
using Application.UseCases.Customers.DTOs;
using Domain.Entities.Customers;

namespace Application.UseCases.Customers.CQRS.Queries.GetById;

public sealed class CustomerGetByIdHandler : IRequestHandler<CustomerGetByIdQuery, OperationResult<CustomerDTO>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerGetByIdHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<CustomerDTO>> Handle(
        CustomerGetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id, cancellationToken);

        if (customer is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: CustomerMessages.NotFound.WithId(request.Id.ToString()));
        }

        var customerDto = _mapper.Map<CustomerDTO>(customer);

        return Result.Success(customerDto);
    }
}
