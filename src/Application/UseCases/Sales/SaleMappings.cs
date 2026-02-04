using Application.UseCases.Sales.DTOs;
using Domain.Entities.Sales;
using Mapster;

namespace Application.UseCases.Sales;

public class SaleMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Sale, SaleDTO>()
            .Map(dest => dest.CustomerName, src => src.Customer != null ? src.Customer.Name : string.Empty)
            .Map(dest => dest.UserName, src => src.User != null ? src.User.Name : string.Empty)
            .Map(dest => dest.Details, src => src.SaleDetails);

        config.NewConfig<SaleDetail, SaleDetailDTO>()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : string.Empty);
    }
}
