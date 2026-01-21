using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Customers.DTOs;
using Domain.Entities.Customers;

namespace Application.UseCases.Customers.CQRS.Queries.GetAll;

public sealed class CustomerGetAllHandler : IRequestHandler<CustomerGetAllQuery, OperationResult<IEnumerable<CustomerDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Customer> _customerRepository;

    public CustomerGetAllHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _customerRepository = unitOfWork.Repository<Customer>();
    }

    public async Task<OperationResult<IEnumerable<CustomerDTO>>> Handle(
        CustomerGetAllQuery request,
        CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);

        var customersDto = _mapper.Map<IEnumerable<CustomerDTO>>(customers);

        return Result.Success(customersDto);
    }
}
