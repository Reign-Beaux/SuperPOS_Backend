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
        var inventories = await _unitOfWork.Repository<Inventory>().GetAllAsync(cancellationToken);
        var inventoryList = inventories.ToList();

        // Optimización: Obtener todos los productos de una vez en lugar de N+1 queries
        var productIds = inventoryList.Select(i => i.ProductId).Distinct().ToList();
        var products = await _unitOfWork.Repository<Product>().QueryAsync(
            predicate: p => productIds.Contains(p.Id),
            cancellationToken: cancellationToken
        );

        // Crear diccionario para búsqueda rápida
        var productDictionary = products.ToDictionary(p => p.Id);

        // Asignar productos a inventarios
        foreach (var inventory in inventoryList)
        {
            if (productDictionary.TryGetValue(inventory.ProductId, out var product))
            {
                inventory.Product = product;
            }
        }

        var dtos = _mapper.Map<List<InventoryDTO>>(inventoryList);
        return Result.Success(dtos);
    }
}
