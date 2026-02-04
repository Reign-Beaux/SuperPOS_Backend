using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Sales.DTOs;
using Domain.Entities.Customers;
using Domain.Entities.Sales;
using Domain.Entities.Users;
using MapsterMapper;

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
        var sale = await _unitOfWork.Repository<Sale>().GetByIdAsync(request.Id, cancellationToken);

        if (sale == null)
        {
            return Result.Error(ErrorResult.NotFound, detail: SaleMessages.NotFound.WithId(request.Id));
        }

        // Cargar relaciones manualmente
        sale.Customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(sale.CustomerId, cancellationToken);
        sale.User = await _unitOfWork.Repository<User>().GetByIdAsync(sale.UserId, cancellationToken);

        // Cargar los detalles
        var saleDetails = await _unitOfWork.Repository<SaleDetail>().QueryAsync(
            sd => sd.SaleId == sale.Id,
            cancellationToken: cancellationToken
        );

        sale.SaleDetails = saleDetails.ToList();

        // Cargar productos de los detalles
        var rules = new SaleRules(_unitOfWork);
        foreach (var detail in sale.SaleDetails)
        {
            detail.Product = await rules.GetProductAsync(detail.ProductId);
        }

        var dto = _mapper.Map<SaleDTO>(sale);
        return Result.Success(dto);
    }
}
