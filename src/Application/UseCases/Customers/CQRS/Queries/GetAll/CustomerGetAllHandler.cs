using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;
using Application.UseCases.Customers.DTOs;
using Domain.Entities.Customers;

namespace Application.UseCases.Customers.CQRS.Queries.GetAll;

public sealed class CustomerGetAllHandler : IRequestHandler<CustomerGetAllQuery, OperationResult<IEnumerable<CustomerDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerGetAllHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<IEnumerable<CustomerDTO>>> Handle(
        CustomerGetAllQuery request,
        CancellationToken cancellationToken)
    {
        var customers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);

        var customersDto = _mapper.Map<IEnumerable<CustomerDTO>>(customers);

        return Result.Success(customersDto);
    }
}
