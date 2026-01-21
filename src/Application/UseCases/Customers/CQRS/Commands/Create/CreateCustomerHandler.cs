using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Customers.DTOs;
using Domain.Entities.Customers;
using MapsterMapper;

namespace Application.UseCases.Customers.CQRS.Commands.Create;

public sealed class CreateCustomerHandler
    : IRequestHandler<CreateCustomerCommand, OperationResult<CustomerDTO>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CustomerRules _customerRules;

    public CreateCustomerHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _customerRules = new CustomerRules(unitOfWork);
    }

    public async Task<OperationResult<CustomerDTO>> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        // Create Customer entity from command
        var customer = new Customer
        {
            Name = request.Name,
            FirstLastname = request.FirstLastname,
            SecondLastname = request.SecondLastname,
            Phone = request.Phone,
            Email = request.Email,
            BirthDate = request.BirthDate
        };

        // Validate uniqueness
        var validationResult = await _customerRules.EnsureUniquenessAsync(
            customer,
            isUpdate: false,
            cancellationToken);

        if (!validationResult.IsSuccess)
            return validationResult;

        // Add to repository
        _unitOfWork.Repository<Customer>().Add(customer);

        // Save changes
        var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (affectedRows == 0)
            return Result.Error(
                ErrorResult.BadRequest,
                detail: CustomerMessages.Create.Failed);

        // Map to DTO and return
        var customerDto = _mapper.Map<CustomerDTO>(customer);

        return new OperationResult<CustomerDTO>(StatusResult.Created, customerDto);
    }
}
