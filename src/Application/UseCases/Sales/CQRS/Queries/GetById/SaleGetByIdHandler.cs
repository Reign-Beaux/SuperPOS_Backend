using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Sales.DTOs;
using Domain.Entities.Sales;
using Domain.Repositories;

namespace Application.UseCases.Sales.CQRS.Queries.GetById;

public class SaleGetByIdHandler : IRequestHandler<SaleGetByIdQuery, OperationResult<SaleDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SaleGetByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OperationResult<SaleDTO>> Handle(SaleGetByIdQuery request, CancellationToken cancellationToken)
    {
        // Use specific repository method that loads all related entities
        var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(request.Id, cancellationToken);

        if (sale == null)
        {
            return Result.Error(ErrorResult.NotFound, detail: SaleMessages.NotFound.WithId(request.Id));
        }

        var dto = _mapper.Map<SaleDTO>(sale);
        return Result.Success(dto);
    }
}
