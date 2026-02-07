using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;
using Application.UseCases.Customers.DTOs;

namespace Application.UseCases.Customers.CQRS.Queries.Search;

public sealed class CustomerSearchHandler : IRequestHandler<CustomerSearchQuery, OperationResult<IEnumerable<CustomerDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerSearchHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<IEnumerable<CustomerDTO>>> Handle(
        CustomerSearchQuery request,
        CancellationToken cancellationToken)
    {
        // Validate minimum search term length
        if (string.IsNullOrWhiteSpace(request.Term) || request.Term.Length < 3)
        {
            return Result.Error(
                ErrorResult.BadRequest,
                detail: "El término de búsqueda debe tener al menos 3 caracteres.");
        }

        var customers = await _unitOfWork.Customers.SearchByNameAsync(request.Term, cancellationToken);

        var customersDto = _mapper.Map<IEnumerable<CustomerDTO>>(customers);

        return Result.Success(customersDto);
    }
}
