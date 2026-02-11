using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Inventories.DTOs;
using Domain.Entities.Inventories;
using Domain.Entities.Products;

namespace Application.UseCases.Inventories.CQRS.Queries.GetByProductId;

public class InventoryGetByProductIdHandler : IRequestHandler<InventoryGetByProductIdQuery, OperationResult<InventoryDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InventoryGetByProductIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OperationResult<InventoryDTO>> Handle(InventoryGetByProductIdQuery request, CancellationToken cancellationToken)
    {
        var inventory = await _unitOfWork.Repository<Inventory>().FirstOrDefaultAsync(
            i => i.ProductId == request.ProductId,
            cancellationToken
        );

        if (inventory is null || inventory.Stock == 0)
        {
            return Result.Error(ErrorResult.NotFound, detail: InventoryMessages.NotFound.WithProductId(request.ProductId));
        }

        // Cargar el producto manualmente
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(inventory.ProductId, cancellationToken);
        if (product != null)
            inventory.Product = product;

        var dto = _mapper.Map<InventoryDTO>(inventory);
        return Result.Success(dto);
    }
}
