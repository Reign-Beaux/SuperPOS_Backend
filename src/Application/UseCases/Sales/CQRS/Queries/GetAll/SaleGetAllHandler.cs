using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Sales.DTOs;
using Domain.Repositories;
using Domain.Specifications.Sales;

namespace Application.UseCases.Sales.CQRS.Queries.GetAll;

public class SaleGetAllHandler : IRequestHandler<SaleGetAllQuery, OperationResult<List<SaleDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SaleGetAllHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OperationResult<List<SaleDTO>>> Handle(SaleGetAllQuery request, CancellationToken cancellationToken)
    {
        // Use Specification pattern to load all sales with related entities
        // (Customer, User, SaleDetails, Products) - demonstrates eager loading
        var specification = new SalesWithDetailsSpecification();
        var sales = await _unitOfWork.Repository<Domain.Entities.Sales.Sale>().ListAsync(specification, cancellationToken);

        var dtos = _mapper.Map<List<SaleDTO>>(sales);
        return Result.Success(dtos);
    }
}
