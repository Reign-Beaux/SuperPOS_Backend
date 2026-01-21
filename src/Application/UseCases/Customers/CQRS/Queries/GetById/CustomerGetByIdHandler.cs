using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Customers.DTOs;
using Domain.Entities.Customers;

namespace Application.UseCases.Customers.CQRS.Queries.GetById;

public sealed class CustomerGetByIdHandler : IRequestHandler<CustomerGetByIdQuery, OperationResult<CustomerDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Customer> _customerRepository;

    public CustomerGetByIdHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _customerRepository = unitOfWork.Repository<Customer>();
    }

    public async Task<OperationResult<CustomerDTO>> Handle(
        CustomerGetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

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
