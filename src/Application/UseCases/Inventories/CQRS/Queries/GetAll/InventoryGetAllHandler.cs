using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Inventories.DTOs;
using Domain.Entities.Inventories;
using Domain.Entities.Products;

namespace Application.UseCases.Inventories.CQRS.Queries.GetAll;

public class InventoryGetAllHandler : IRequestHandler<InventoryGetAllQuery, OperationResult<List<InventoryDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InventoryGetAllHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OperationResult<List<InventoryDTO>>> Handle(InventoryGetAllQuery request, CancellationToken cancellationToken)
    {
        var inventories = await _unitOfWork.Repository<Inventory>().GetAllAsync(cancellationToken);

        // Cargar los productos manualmente
        var inventoryList = inventories.ToList();
        foreach (var inventory in inventoryList)
        {
            var currentProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(inventory.ProductId, cancellationToken);
            if (currentProduct is not null)
                inventory.Product = currentProduct;
        }

        var dtos = _mapper.Map<List<InventoryDTO>>(inventoryList);
        return Result.Success(dtos);
    }
}
