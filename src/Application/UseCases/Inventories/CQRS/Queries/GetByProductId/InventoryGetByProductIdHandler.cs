using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Inventories.DTOs;
using Domain.Entities.Inventories;
using Domain.Entities.Products;
using MapsterMapper;

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

        if (inventory == null)
        {
            return Result.Error(ErrorResult.NotFound, detail: InventoryMessages.NotFound.WithProductId(request.ProductId));
        }

        // Cargar el producto manualmente
        inventory.Product = await _unitOfWork.Repository<Product>().GetByIdAsync(inventory.ProductId, cancellationToken);

        var dto = _mapper.Map<InventoryDTO>(inventory);
        return Result.Success(dto);
    }
}
