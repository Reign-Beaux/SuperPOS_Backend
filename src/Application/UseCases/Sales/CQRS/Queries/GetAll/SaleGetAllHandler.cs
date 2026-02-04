using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Sales.DTOs;
using Domain.Repositories;

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
        // Use ISaleRepository.GetAllWithDetailsAsync which loads all relationships through the aggregate
        var sales = await _unitOfWork.Sales.GetAllWithDetailsAsync(cancellationToken);

        var dtos = _mapper.Map<List<SaleDTO>>(sales);
        return Result.Success(dtos);
    }
}
