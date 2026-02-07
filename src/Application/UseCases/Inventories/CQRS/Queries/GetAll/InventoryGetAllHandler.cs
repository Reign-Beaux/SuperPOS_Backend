using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;
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
        // Obtener TODOS los productos (la base del inventario)
        var products = await _unitOfWork.Repository<Product>().GetAllAsync(cancellationToken);
        var productList = products.ToList();

        // Obtener todos los registros de inventario existentes
        var inventories = await _unitOfWork.Repository<Inventory>().GetAllAsync(cancellationToken);
        var inventoryDictionary = inventories.ToDictionary(i => i.ProductId);

        // Crear DTOs para todos los productos
        var dtos = new List<InventoryDTO>();

        foreach (var product in productList)
        {
            // Buscar si existe un registro de inventario para este producto
            if (inventoryDictionary.TryGetValue(product.Id, out var inventory))
            {
                // Producto con inventario existente
                inventory.Product = product;
                var dto = _mapper.Map<InventoryDTO>(inventory);
                dtos.Add(dto);
            }
            else
            {
                // Producto sin inventario - crear DTO con stock = 0
                var dto = new InventoryDTO(
                    Id: Guid.Empty, // No hay registro de inventario
                    ProductId: product.Id,
                    ProductName: product.Name,
                    ProductDescription: product.Description,
                    Barcode: product.Barcode,
                    Stock: 0, // Stock en cero
                    CreatedAt: product.CreatedAt,
                    UpdatedAt: product.UpdatedAt ?? product.CreatedAt
                );
                dtos.Add(dto);
            }
        }

        return Result.Success(dtos);
    }
}
