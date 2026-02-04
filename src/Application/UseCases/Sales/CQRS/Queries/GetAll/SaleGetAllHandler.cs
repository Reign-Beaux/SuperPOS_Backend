using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Sales.DTOs;
using Domain.Entities.Customers;
using Domain.Entities.Sales;
using Domain.Entities.Users;

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
        var sales = await _unitOfWork.Repository<Sale>().GetAllAsync(cancellationToken);
        var salesList = sales.ToList();

        var rules = new SaleRules(_unitOfWork);

        // Cargar relaciones para cada venta
        foreach (var sale in salesList)
        {
            sale.Customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(sale.CustomerId, cancellationToken);
            sale.User = await _unitOfWork.Repository<User>().GetByIdAsync(sale.UserId, cancellationToken);

            // Cargar los detalles
            var saleDetails = await _unitOfWork.Repository<SaleDetail>().QueryAsync(
                sd => sd.SaleId == sale.Id,
                cancellationToken: cancellationToken
            );

            sale.SaleDetails = saleDetails.ToList();

            // Cargar productos de los detalles
            foreach (var detail in sale.SaleDetails)
            {
                detail.Product = await rules.GetProductAsync(detail.ProductId);
            }
        }

        var dtos = _mapper.Map<List<SaleDTO>>(salesList);
        return Result.Success(dtos);
    }
}
