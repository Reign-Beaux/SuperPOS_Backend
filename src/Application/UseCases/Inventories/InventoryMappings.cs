using Application.UseCases.Inventories.DTOs;
using Domain.Entities.Inventories;
using Mapster;

namespace Application.UseCases.Inventories;

public class InventoryMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Inventory, InventoryDTO>()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : string.Empty);
    }
}
