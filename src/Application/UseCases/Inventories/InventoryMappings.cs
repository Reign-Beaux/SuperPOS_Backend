using Application.UseCases.Inventories.DTOs;
using Domain.Entities.Inventories;
using Mapster;

namespace Application.UseCases.Inventories;

public class InventoryMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Inventory, InventoryDTO>()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : string.Empty)
            .Map(dest => dest.ProductDescription, src => src.Product != null ? src.Product.Description : null)
            .Map(dest => dest.Barcode, src => src.Product != null ? src.Product.Barcode : null)
            .Map(dest => dest.UnitPrice, src => src.Product != null ? src.Product.UnitPrice : 0)
            .Map(dest => dest.Stock, src => src.Stock);
    }
}
